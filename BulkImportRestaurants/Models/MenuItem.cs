using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BulkImportRestaurants.Models
{
    public class MenuItem : IItemValidating
    {
        [Key]
        public string Guid { get; set; }   // this is the JSON "id" like M-2001

        [Required]
        public string Title { get; set; }

        [Required]
        public double Price { get; set; }

        [Required]
        public string Currency { get; set; }

        [Required]
        public string RestaurantId { get; set; }

        [Required]
        public string Status { get; set; } = "pending";

        public List<string> GetValidators()
        {
            // Validator depends on restaurant owner (handled in ApprovalFilter)
            return new List<string>();
        }

        public string GetCardPartial()
        {
            return "_MenuItemCard";
        }
    }
}
