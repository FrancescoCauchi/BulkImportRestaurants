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
    [Authorize]
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

            var items = ImportItemFactory.Create(json);

            _memoryRepo.SaveItems(items);

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

            // 1) Create folders + copy default image + set Restaurant.ImagePath
            var zipRelativePath = CreateZipForItemsAndSetImagePaths(items);

            // 2) Save items to DB AFTER image paths are set
            _dbRepo.SaveItems(items);

            // 3) Clear preview
            _memoryRepo.Clear();

            TempData["Message"] = "Items saved to database and pending approval.";
            TempData["ZipPath"] = zipRelativePath; // optional for download button
            return RedirectToAction("BulkImport");
        }

        /// <summary>
        /// Creates /wwwroot/uploads/{importId}/{RestaurantId}/default.jpg and zips it to /wwwroot/uploads/{importId}.zip
        /// Also sets Restaurant.ImagePath so the catalog can show images.
        /// Returns zip path like "/uploads/{importId}.zip"
        /// </summary>
        private string CreateZipForItemsAndSetImagePaths(System.Collections.Generic.List<IItemValidating> items)
        {
            var importId = Guid.NewGuid().ToString("N");
            var webRoot = _env.WebRootPath;

            // Ensure uploads root exists
            var uploadsRoot = Path.Combine(webRoot, "uploads");
            Directory.CreateDirectory(uploadsRoot);

            // Create import folder
            var importFolder = Path.Combine(uploadsRoot, importId);
            Directory.CreateDirectory(importFolder);

            // Default image source
            var defaultImagePath = Path.Combine(webRoot, "images", "default.jpg");
            var hasDefault = System.IO.File.Exists(defaultImagePath);

            // Create restaurant folders + copy image + set DB image path
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
                    System.IO.File.Copy(defaultImagePath, destImagePath, overwrite: true);

                    // Web path stored in DB
                    r.ImagePath = $"/uploads/{importId}/{r.Id}/default.jpg";
                }
                else
                {
                    // If default.jpg doesn't exist, keep it null (no image displayed)
                    r.ImagePath = null;
                }
            }

            // Create zip from import folder
            var zipFileName = importId + ".zip";
            var zipFullPath = Path.Combine(uploadsRoot, zipFileName);

            if (System.IO.File.Exists(zipFullPath))
                System.IO.File.Delete(zipFullPath);

            ZipFile.CreateFromDirectory(importFolder, zipFullPath);

            return "/uploads/" + zipFileName;
        }
    }
}
