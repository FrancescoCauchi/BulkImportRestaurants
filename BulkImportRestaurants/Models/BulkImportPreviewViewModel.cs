using System.Collections.Generic;

namespace BulkImportRestaurants.Models
{
    public class BulkImportPreviewViewModel
    {
        public List<Restaurant> Restaurants { get; set; } = new();
        public List<MenuItem> MenuItems { get; set; } = new();
    }
}
