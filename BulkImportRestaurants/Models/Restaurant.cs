using System.ComponentModel.DataAnnotations;

namespace BulkImportRestaurants.Models
{
    public class Restaurant : IItemValidating
    {
        [Key]
        public string Id { get; set; }   // Example: R-1001

        public string Name { get; set; }
        public string OwnerEmailAddress { get; set; }
        public string Status { get; set; } = "pending";

        public string Description { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }

        // Required by the assignment
        public List<string> GetValidators()
        {
            return new List<string>
            {
                OwnerEmailAddress,
                "siteadmin@example.com"
            };
        }

        public string GetCardPartial()
        {
            return "_RestaurantCard";
        }
    }
}
