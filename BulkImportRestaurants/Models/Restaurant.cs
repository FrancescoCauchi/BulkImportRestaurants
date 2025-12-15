using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BulkImportRestaurants.Models
{
    public class Restaurant : IItemValidating
    {
        [Key]
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public string OwnerEmailAddress { get; set; }

        public string Address { get; set; }

        public string Phone { get; set; }

        [Required]
        public string Status { get; set; } = "pending";

        public List<string> GetValidators()
        {
            // Hard-coded admin email (as allowed by spec)
            return new List<string> { "admin@example.com" };
        }

        public string GetCardPartial()
        {
            return "_RestaurantCard";
        }
    }
}
