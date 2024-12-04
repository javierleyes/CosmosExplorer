using CosmosExplorer.Core;
using Microsoft.Azure.Cosmos;

namespace CosmosExplorer.UI.Common
{
    public static class SharedProperties
    {
        public static CosmosExplorerCore CosmosExplorerCore { get; set; }

        public static DatabaseTreeCollection DatabaseCollection { get; set; }

        public static ItemListViewCollection ItemListViewCollection { get; set; }

        public static Dictionary<string, string> ContainerPartitionKey { get; set; }

        public static string SelectedDatabase { get; set; }

        public static string SelectedContainer { get; set; }

        public static async Task<List<string>> GetDatabases()
        {
            List<string> databaseNames = new List<string>();

            FeedIterator<DatabaseProperties> iterator = CosmosExplorerCore.GetDatabaseIterator();

            while (iterator.HasMoreResults)
            {
                FeedResponse<DatabaseProperties> databases = await iterator.ReadNextAsync().ConfigureAwait(true);
                foreach (var database in databases)
                {
                    databaseNames.Add(database.Id);
                }
            }

            return databaseNames;
        }

        public static async Task<Dictionary<string, List<ContainerInformation>>> GetDatabasesInformationAsync()
        {
            Dictionary<string, List<ContainerInformation>> databases = new Dictionary<string, List<ContainerInformation>>();
            ContainerPartitionKey = new Dictionary<string, string>();

            List<string> databaseNames = await GetDatabases().ConfigureAwait(true);
            foreach (var database in databaseNames)
            {
                List<ContainerInformation> containers = new List<ContainerInformation>();

                FeedIterator<ContainerProperties> containerIterator = CosmosExplorerCore.GetContainerIterator(database);
                while (containerIterator.HasMoreResults)
                {
                    FeedResponse<ContainerProperties> containerResponse = await containerIterator.ReadNextAsync().ConfigureAwait(true);
                    foreach (var container in containerResponse)
                    {
                        ContainerInformation containerInformation = new ContainerInformation(container.Id, database, container.PartitionKeyPath);
                        containers.Add(containerInformation);

                        ContainerPartitionKey.Add(container.Id, container.PartitionKeyPath);
                    }
                }

                databases.Add(database, containers);
            }

            return databases;
        }

        public static async Task LoadDatabasesAsync()
        {
            Dictionary<string, List<ContainerInformation>> databases = await GetDatabasesInformationAsync().ConfigureAwait(true);
            DatabaseCollection.LoadDatabases(databases);
        }

        public static async Task LoadItemsAsync(string databaseName, string containerName)
        {
            ItemListViewCollection.Clear();

            string query = "SELECT TOP 10 * FROM c";
            List<Tuple<string, string>> items = new List<Tuple<string, string>>();

            FeedIterator<dynamic> iterator = CosmosExplorerCore.GetQueryIterator(databaseName, containerName, query);

            while (iterator.HasMoreResults)
            {
                FeedResponse<dynamic> response = await iterator.ReadNextAsync().ConfigureAwait(true);
                foreach (var item in response)
                {
                    string partitionKey = ContainerPartitionKey[containerName].TrimStart('/');
                    items.Add(new Tuple<string, string>(item["id"].ToString(), item[partitionKey].ToString()));
                }
            }

            ItemListViewCollection.LoadItems(items);
        }
    }
}
