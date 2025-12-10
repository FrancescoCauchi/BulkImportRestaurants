using System.Collections.Generic;
using BulkImportRestaurants.Models;
using Newtonsoft.Json.Linq;

namespace BulkImportRestaurants.Services
{
    public static class ImportItemFactory
    {
        public static List<IItemValidating> Create(string json)
        {
            var list = new List<IItemValidating>();
            var arr = JArray.Parse(json);

            foreach (var item in arr)
            {
                string type = item["type"]?.ToString() ?? "";

                if (type == "restaurant")
                {
                    list.Add(new Restaurant
                    {
                        Id = item["id"]?.ToString(),
                        Name = item["name"]?.ToString(),
                        Description = item["description"]?.ToString(),
                        OwnerEmailAddress = item["ownerEmailAddress"]?.ToString(),
                        Address = item["address"]?.ToString(),
                        Phone = item["phone"]?.ToString(),
                        Status = "pending"
                    });
                }

                if (type == "menuitem")
                {
                    list.Add(new MenuItem
                    {
                        Guid = item["id"]?.ToString(),
                        Title = item["title"]?.ToString(),
                        Price = (double?)item["price"] ?? 0,
                        Currency = item["currency"]?.ToString(),
                        RestaurantId = item["restaurantId"]?.ToString(),
                        Status = "pending"
                    });
                }
            }

            return list;
        }
    }
}
