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

        // ------------------------------------------------------------
        // IItemsRepository implementation
        // ------------------------------------------------------------

        // For the Catalog: return ONLY approved items
        public List<IItemValidating> GetItems()
        {
            var restaurants = _db.Restaurants
                .Where(r => r.Status == "approved")
                .Cast<IItemValidating>()
                .ToList();

            var menus = _db.MenuItems
                .Where(m => m.Status == "approved")
                .Cast<IItemValidating>()
                .ToList();

            return restaurants.Concat(menus).ToList();
        }

        // Save pending items from in-memory import
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
            // Nothing needed for DB repository
        }

        // ------------------------------------------------------------
        // Catalog Support (Approved Items)
        // ------------------------------------------------------------

        public List<Restaurant> GetApprovedRestaurants()
        {
            return _db.Restaurants
                .Where(r => r.Status == "approved")
                .ToList();
        }

        public List<MenuItem> GetApprovedMenuItemsForRestaurant(string restaurantId)
        {
            return _db.MenuItems
                .Where(m => m.Status == "approved" && m.RestaurantId == restaurantId)
                .ToList();
        }

        // ------------------------------------------------------------
        // Approval System
        // ------------------------------------------------------------

        // Restaurants pending admin approval
        public List<Restaurant> GetPendingRestaurants()
        {
            return _db.Restaurants
                .Where(r => r.Status == "pending")
                .ToList();
        }

        // Menu items pending restaurant owner approval
        public List<MenuItem> GetPendingMenuItemsForOwner(string ownerEmail)
        {
            var query =
                from m in _db.MenuItems
                join r in _db.Restaurants on m.RestaurantId equals r.Id
                where m.Status == "pending" && r.OwnerEmailAddress == ownerEmail
                select m;

            return query.ToList();
        }

        // Approve a restaurant
        public void ApproveRestaurant(string id)
        {
            var restaurant = _db.Restaurants.FirstOrDefault(r => r.Id == id);
            if (restaurant != null)
            {
                restaurant.Status = "approved";
                _db.SaveChanges();
            }
        }

        // Approve a menu item
        public void ApproveMenuItem(string guid)
        {
            var menuItem = _db.MenuItems.FirstOrDefault(m => m.Guid == guid);
            if (menuItem != null)
            {
                menuItem.Status = "approved";
                _db.SaveChanges();
            }
        }
    }
}
