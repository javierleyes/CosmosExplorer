using System.Collections.ObjectModel;

namespace CosmosExplorer.UI
{
    public class DatabaseTreeSource
    {
        public string Name { get; set; }
    }

    public class DatabaseTreeCollection : ObservableCollection<DatabaseTreeSource>
    {
        public void LoadDatabases(List<string> databaseNames)
        {
            foreach (var databaseName in databaseNames)
            {
                this.Add(new DatabaseTreeSource { Name = databaseName });
            }
        }
    }
}
