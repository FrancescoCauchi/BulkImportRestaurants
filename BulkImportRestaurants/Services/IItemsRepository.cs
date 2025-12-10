using System.Collections.Generic;
using BulkImportRestaurants.Models;

namespace BulkImportRestaurants.Services
{
    public interface IItemsRepository
    {
        List<IItemValidating> GetItems();
        void SaveItems(List<IItemValidating> items);
        void Clear();
    }
}
