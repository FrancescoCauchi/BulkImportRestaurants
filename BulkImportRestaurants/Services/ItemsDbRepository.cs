using System.Collections.Generic;
using System.Linq;
using BulkImportRestaurants.Data;
using BulkImportRestaurants.Models;

namespace BulkImportRestaurants.Services
{
    public class ItemsDbRepository : IItemsRepository
    {
        private readonly ApplicationDbContext _db;

        public ItemsDbRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public List<IItemValidating> GetItems()
        {
            // Return only approved items (for catalog)
            var restaurants = _db.Restaurants.Where(r => r.Status == "approved").Cast<IItemValidating>().ToList();
            var menus = _db.MenuItems.Where(m => m.Status == "approved").Cast<IItemValidating>().ToList();

            return restaurants.Concat(menus).ToList();
        }

        public void SaveItems(List<IItemValidating> items)
        {
            foreach (var item in items)
            {
                if (item is Restaurant r)
                    _db.Restaurants.Add(r);

                if (item is MenuItem m)
                    _db.MenuItems.Add(m);
            }

            _db.SaveChanges();
        }

        public void Clear()
        {
            // Nothing needed for DB
        }
    }
}
