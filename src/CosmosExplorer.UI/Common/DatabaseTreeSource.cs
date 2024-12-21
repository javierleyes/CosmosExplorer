using CosmosExplorer.UI.Common;
using System.Collections.ObjectModel;

namespace CosmosExplorer.UI
{
    public class ContainerTreeSource
    {
        public string Name { get; set; }

        public string Database { get; set; }

        public string PartitionKey { get; set; }

        public ContainerTreeSource(string name, string database, string partitionKey)
        {
            Name = name;
            Database = database;
            PartitionKey = partitionKey;
        }
    }

    public class DatabaseTreeSource
    {
        public string Name { get; set; }

        public ObservableCollection<ContainerTreeSource> Containers { get; set; }

        public DatabaseTreeSource(string name, List<ContainerInformation> containers)
        {
            Name = name;

            Containers = new ObservableCollection<ContainerTreeSource>();
            foreach (ContainerInformation container in containers)
            {
                Containers.Add(new ContainerTreeSource(container.Name, container.Database, container.PartitionKey));
            }
        }
    }

    public class DatabaseTreeCollection : ObservableCollection<DatabaseTreeSource>
    {
        public void LoadDatabases(Dictionary<string, List<ContainerInformation>> databases)
        {
            this.Clear();

            foreach (string database in databases.Keys)
            {
                this.Add(new DatabaseTreeSource (database, databases[database]));
            }
        }
    }
}
