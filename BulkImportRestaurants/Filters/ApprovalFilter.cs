using System.Linq;
using System.Threading.Tasks;
using BulkImportRestaurants.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BulkImportRestaurants.Filters
{
    public class ApprovalFilter : IAsyncActionFilter
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public ApprovalFilter(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = await _userManager.GetUserAsync(context.HttpContext.User);
            var email = user?.Email ?? "";

            // Approving restaurant?
            if (context.ActionArguments.ContainsKey("id"))
            {
                var id = context.ActionArguments["id"]?.ToString();
                var restaurant = _db.Restaurants.FirstOrDefault(r => r.Id == id);

                if (restaurant == null)
                {
                    context.Result = new NotFoundResult();
                    return;
                }

                // Only site admin can approve restaurants
                if (email != "admin@example.com")
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }

            // Approving menu item?
            if (context.ActionArguments.ContainsKey("guid"))
            {
                var guid = context.ActionArguments["guid"]?.ToString();
                var menu = _db.MenuItems.FirstOrDefault(m => m.Guid == guid);

                if (menu == null)
                {
                    context.Result = new NotFoundResult();
                    return;
                }

                var restaurant = _db.Restaurants.FirstOrDefault(r => r.Id == menu.RestaurantId);

                if (restaurant == null)
                {
                    context.Result = new NotFoundResult();
                    return;
                }

                // Only owner of that restaurant can approve menu items
                if (email != restaurant.OwnerEmailAddress)
                {
                    context.Result = new ForbidResult();
                    return;
                }
            }

            await next();
        }
    }
}
