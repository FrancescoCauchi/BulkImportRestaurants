using System.ComponentModel.DataAnnotations;

namespace BulkImportRestaurants.Models
{
    public class MenuItem : IItemValidating
    {
        [Key]
        public string Guid { get; set; }  // Example: M-2001

        public string Title { get; set; }
        public double Price { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; } = "pending";

        public string RestaurantId { get; set; }

        public List<string> GetValidators()
        {
            return new List<string>();
        }

        public string GetCardPartial()
        {
            return "_MenuItemCard";
        }
    }
}
