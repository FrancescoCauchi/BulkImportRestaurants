using BulkImportRestaurants.Services;
using Microsoft.AspNetCore.Mvc;

namespace BulkImportRestaurants.Controllers
{
    public class CatalogController : Controller
    {
        private readonly ItemsDbRepository _dbRepo;

        public CatalogController(ItemsDbRepository dbRepo)
        {
            _dbRepo = dbRepo;
        }

        // GET: /Catalog
        public IActionResult Index()
        {
            var restaurants = _dbRepo.GetApprovedRestaurants();
            return View(restaurants);
        }

        // GET: /Catalog/Details/R-1001
        public IActionResult Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var restaurant = _dbRepo.GetApprovedRestaurants()
                                    .FirstOrDefault(r => r.Id == id);

            if (restaurant == null)
                return NotFound();

            var menuItems = _dbRepo.GetApprovedMenuItemsForRestaurant(id);
            ViewBag.Restaurant = restaurant;
            return View(menuItems);
        }
    }
}
