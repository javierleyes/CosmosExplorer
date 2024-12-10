using System.Collections.ObjectModel;

namespace CosmosExplorer.UI.Common
{
    public class ItemListViewSource
    {
        public string Id { get; set; }
        public string PartitionKey { get; set; }

        public ItemListViewSource(string id, string partitionKey)
        {
            Id = id;
            PartitionKey = partitionKey;
        }
    }

    public class ItemListViewCollection : ObservableCollection<ItemListViewSource>
    {
        public void LoadItems(IEnumerable<Tuple<string, string>> items)
        {
            foreach (Tuple<string, string> item in items)
            {
                this.Add(new ItemListViewSource(item.Item1, item.Item2));
            }
        }

        public void AddItem(string id, string partitionKey)
        {
            this.Add(new ItemListViewSource(id, partitionKey));
        }
    }
}
