using System;                // add if not already there
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

        // existing methods...

        public List<IItemValidating> GetItems()
        {
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
        }

        // 🔹 NEW: approve restaurant by id
        public void ApproveRestaurant(string id)
        {
            var restaurant = _db.Restaurants.FirstOrDefault(r => r.Id == id);
            if (restaurant != null)
            {
                restaurant.Status = "approved";
                _db.SaveChanges();
            }
        }

        // 🔹 NEW: approve menu item by guid
        public void ApproveMenuItem(string guid)
        {
            var menuItem = _db.MenuItems.FirstOrDefault(m => m.Guid == guid);
            if (menuItem != null)
            {
                menuItem.Status = "approved";
                _db.SaveChanges();
            }
        }

        // 🔹 Helpers to get pending items
        public List<Restaurant> GetPendingRestaurants()
        {
            return _db.Restaurants.Where(r => r.Status == "pending").ToList();
        }

        public List<MenuItem> GetPendingMenuItemsForOwner(string ownerEmail)
        {
            // menuitems whose restaurant is owned by this user
            var query =
                from m in _db.MenuItems
                join r in _db.Restaurants on m.RestaurantId equals r.Id
                where m.Status == "pending" && r.OwnerEmailAddress == ownerEmail
                select m;

            return query.ToList();
        }
    }
}
