using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using BulkImportRestaurants.Models;
using BulkImportRestaurants.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace BulkImportRestaurants.Controllers
{
    [Authorize] // user must be logged in to use bulk import
    public class BulkImportController : Controller
    {
        private readonly ItemsInMemoryRepository _memoryRepo;
        private readonly ItemsDbRepository _dbRepo;
        private readonly IWebHostEnvironment _env;

        public BulkImportController(
            ItemsInMemoryRepository memoryRepo,
            ItemsDbRepository dbRepo,
            IWebHostEnvironment env)
        {
            _memoryRepo = memoryRepo;
            _dbRepo = dbRepo;
            _env = env;
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

            // Create ZIP structure under wwwroot/uploads
            var zipRelativePath = CreateZipForItems(items);

            // Clear the in-memory store
            _memoryRepo.Clear();

            TempData["Message"] = $"Items saved to database and pending approval. ZIP generated at: {zipRelativePath}";
            return RedirectToAction("BulkImport");
        }

        /// <summary>
        /// Creates a folder per restaurant with default.jpg and zips it.
        /// Returns the relative path to the zip file (for showing a link later).
        /// </summary>
        private string CreateZipForItems(System.Collections.Generic.List<IItemValidating> items)
        {
            // Unique id for this import
            var importId = Guid.NewGuid().ToString("N");

            // wwwroot path
            var webRoot = _env.WebRootPath;

            // Base folder for this import
            var uploadsRoot = Path.Combine(webRoot, "uploads");
            Directory.CreateDirectory(uploadsRoot);

            var importFolder = Path.Combine(uploadsRoot, importId);
            Directory.CreateDirectory(importFolder);

            // Path to default image
            var defaultImagePath = Path.Combine(webRoot, "images", "default.jpg");
            bool hasDefault = System.IO.File.Exists(defaultImagePath);

            // For each restaurant, create a folder and copy default.jpg (if exists)
            var restaurants = items.OfType<Restaurant>().ToList();

            foreach (var r in restaurants)
            {
                if (string.IsNullOrWhiteSpace(r.Id))
                    continue;

                var restaurantFolder = Path.Combine(importFolder, r.Id);
                Directory.CreateDirectory(restaurantFolder);

                if (hasDefault)
                {
                    var destImagePath = Path.Combine(restaurantFolder, "default.jpg");
                    // Overwrite if exists
                    System.IO.File.Copy(defaultImagePath, destImagePath, overwrite: true);
                }
            }

            // Create zip file from the import folder
            var zipFileName = importId + ".zip";
            var zipFullPath = Path.Combine(uploadsRoot, zipFileName);

            if (System.IO.File.Exists(zipFullPath))
            {
                System.IO.File.Delete(zipFullPath);
            }

            ZipFile.CreateFromDirectory(importFolder, zipFullPath);

            // Return a path that can be used in <a href> later
            var relativeZipPath = "/uploads/" + zipFileName;
            return relativeZipPath;
        }
    }
}
