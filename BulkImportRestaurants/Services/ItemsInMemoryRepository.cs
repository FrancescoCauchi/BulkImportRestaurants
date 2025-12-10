using System.Collections.Generic;
using BulkImportRestaurants.Models;

namespace BulkImportRestaurants.Services
{
    public class ItemsInMemoryRepository : IItemsRepository
    {
        private readonly List<IItemValidating> _items = new();

        public List<IItemValidating> GetItems()
        {
            return _items;
        }

        public void SaveItems(List<IItemValidating> items)
        {
            _items.Clear();
            _items.AddRange(items);
        }

        public void Clear()
        {
            _items.Clear();
        }
    }
}
