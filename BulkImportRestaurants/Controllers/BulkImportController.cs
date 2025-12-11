using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BulkImportRestaurants.Models;
using BulkImportRestaurants.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkImportRestaurants.Controllers
{
    [Authorize] // user must be logged in to use bulk import
    public class BulkImportController : Controller
    {
        private readonly ItemsInMemoryRepository _memoryRepo;
        private readonly ItemsDbRepository _dbRepo;

        public BulkImportController(ItemsInMemoryRepository memoryRepo, ItemsDbRepository dbRepo)
        {
            _memoryRepo = memoryRepo;
            _dbRepo = dbRepo;
        }

        // GET: /BulkImport/BulkImport
        [HttpGet]
        public IActionResult BulkImport()
        {
            return View();
        }

        // POST: /BulkImport/BulkImport
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkImport(IFormFile jsonFile, string jsonText)
        {
            if (jsonFile == null && string.IsNullOrWhiteSpace(jsonText))
            {
                ModelState.AddModelError(string.Empty, "Please upload a JSON file or paste JSON text.");
                return View();
            }

            string json;

            if (jsonFile != null)
            {
                using var reader = new StreamReader(jsonFile.OpenReadStream());
                json = await reader.ReadToEndAsync();
            }
            else
            {
                json = jsonText;
            }

            // Use the factory to build objects
            var items = ImportItemFactory.Create(json);

            // Save to in-memory repository for preview
            _memoryRepo.SaveItems(items);

            // Build view model for the preview page
            var vm = new BulkImportPreviewViewModel
            {
                Restaurants = items.OfType<Restaurant>().ToList(),
                MenuItems = items.OfType<MenuItem>().ToList()
            };

            return View("Preview", vm);
        }

        // POST: /BulkImport/Commit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Commit()
        {
            var items = _memoryRepo.GetItems();

            if (items == null || !items.Any())
            {
                TempData["Error"] = "No items to commit. Please upload JSON first.";
                return RedirectToAction("BulkImport");
            }

            // Save to DB (status remains 'pending')
            _dbRepo.SaveItems(items);

            // Clear the in-memory store
            _memoryRepo.Clear();

            TempData["Message"] = "Items saved to database and pending approval.";
            return RedirectToAction("BulkImport");
        }
    }
}
