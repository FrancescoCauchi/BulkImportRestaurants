using System.Threading.Tasks;
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
            IConfiguration config)
        {
            _dbRepo = dbRepo;
            _userManager = userManager;
            _adminEmail = config["AdminEmail"]; // from appsettings.json
        }

        // GET: /Items/Verification
        public async Task<IActionResult> Verification()
        {
            var user = await _userManager.GetUserAsync(User);
            var email = user?.Email ?? "";

            if (email == _adminEmail)
            {
                // Site admin: see pending restaurants
                var pendingRestaurants = _dbRepo.GetPendingRestaurants();
                return View("PendingRestaurants", pendingRestaurants);
            }
            else
            {
                // Restaurant owner: see pending menuitems for their restaurants
                var pendingMenus = _dbRepo.GetPendingMenuItemsForOwner(email);
                return View("PendingMenuItems", pendingMenus);
            }
        }

        // POST: /Items/ApproveRestaurant/R-1001
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveRestaurant(string id)
        {
            _dbRepo.ApproveRestaurant(id);
            return RedirectToAction("Verification");
        }

        // POST: /Items/ApproveMenuItem/M-2001
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ApproveMenuItem(string guid)
        {
            _dbRepo.ApproveMenuItem(guid);
            return RedirectToAction("Verification");
        }
    }
}
