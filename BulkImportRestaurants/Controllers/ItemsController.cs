using System.Threading.Tasks;
using BulkImportRestaurants.Filters;
using BulkImportRestaurants.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace BulkImportRestaurants.Controllers
{
    [Authorize]
    public class ItemsController : Controller
    {
        private readonly ItemsDbRepository _dbRepo;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly string _adminEmail;

        public ItemsController(
            ItemsDbRepository dbRepo,
            UserManager<IdentityUser> userManager,
            IConfiguration configuration)
        {
            _dbRepo = dbRepo;
            _userManager = userManager;
            _adminEmail = configuration["AdminEmail"];
        }

        // ---------------------------------------------------------
        // Verification page (decides what to show based on user)
        // ---------------------------------------------------------
        // GET: /Items/Verification
        public async Task<IActionResult> Verification()
        {
            var user = await _userManager.GetUserAsync(User);
            var email = user?.Email ?? "";

            // SITE ADMIN → pending restaurants
            if (email == _adminEmail)
            {
                var pendingRestaurants = _dbRepo.GetPendingRestaurants();
                return View("PendingRestaurants", pendingRestaurants);
            }

            // RESTAURANT OWNER → pending menu items
            var pendingMenuItems = _dbRepo.GetPendingMenuItemsForOwner(email);
            return View("PendingMenuItems", pendingMenuItems);
        }

        // ---------------------------------------------------------
        // Approve Restaurant (ADMIN ONLY)
        // ---------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(ApprovalFilter))]
        public IActionResult ApproveRestaurant(string id)
        {
            _dbRepo.ApproveRestaurant(id);
            return RedirectToAction("Verification");
        }

        // ---------------------------------------------------------
        // Approve Menu Item (OWNER ONLY)
        // ---------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ServiceFilter(typeof(ApprovalFilter))]
        public IActionResult ApproveMenuItem(string guid)
        {
            _dbRepo.ApproveMenuItem(guid);
            return RedirectToAction("Verification");
        }
    }
}
