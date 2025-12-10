namespace BulkImportRestaurants.Models
{
    public interface IItemValidating
    {
        List<string> GetValidators();
        string GetCardPartial();
    }
}
