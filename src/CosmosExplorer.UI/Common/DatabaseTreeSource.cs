using System.Collections.ObjectModel;

namespace CosmosExplorer.UI
{
    public class ContainerTreeSource
    {
        public string Name { get; set; }

        public ContainerTreeSource(string name)
        {
             Name = name;
        }
    }

    public class DatabaseTreeSource
    {
        public string Name { get; set; }

        public ObservableCollection<ContainerTreeSource> Containers { get; set; }

        public DatabaseTreeSource(string name, List<string> containerNames)
        {
            Name = name;

            Containers = new ObservableCollection<ContainerTreeSource>();
            foreach (var containerName in containerNames)
            {
                Containers.Add(new ContainerTreeSource(containerName));
            }
        }
    }

    public class DatabaseTreeCollection : ObservableCollection<DatabaseTreeSource>
    {
        public void LoadDatabases(Dictionary<string, List<string>> databases)
        {
            foreach (string database in databases.Keys)
            {
                this.Add(new DatabaseTreeSource (database, databases[database]));
            }
        }
    }
}
