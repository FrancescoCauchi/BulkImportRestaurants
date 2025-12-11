using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BulkImportRestaurants.Models;
using BulkImportRestaurants.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkImportRestaurants.Controllers
{
    [Authorize] // user must be logged in to bulk import
    public class BulkImportController : Controller
    {
        private readonly ItemsInMemoryRepository _memoryRepo;

        public BulkImportController(ItemsInMemoryRepository memoryRepo)
        {
            _memoryRepo = memoryRepo;
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

            // Use the Factory to build objects
            var items = ImportItemFactory.Create(json);

            // Save to in-memory repo for preview
            _memoryRepo.SaveItems(items);

            // Build view model
            var vm = new BulkImportPreviewViewModel
            {
                Restaurants = items.OfType<Restaurant>().ToList(),
                MenuItems = items.OfType<MenuItem>().ToList()
            };

            return View("Preview", vm);
        }
    }
}
