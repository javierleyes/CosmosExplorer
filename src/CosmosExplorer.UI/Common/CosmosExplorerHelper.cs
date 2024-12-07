using CosmosExplorer.Core;
using Microsoft.Azure.Cosmos;

namespace CosmosExplorer.UI.Common
{
    public static class CosmosExplorerHelper
    {
        private static CosmosExplorerCore CosmosExplorerCore { get; set; }

        public static void Initialize(string connectionString)
        {
            CosmosExplorerCore = new CosmosExplorerCore(connectionString);
        }

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
            SharedProperties.ContainerPartitionKey = new Dictionary<string, string>();

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

                        SharedProperties.ContainerPartitionKey.Add(container.Id, container.PartitionKeyPath);
                    }
                }

                databases.Add(database, containers);
            }

            return databases;
        }

        public static async Task LoadDatabasesAsync()
        {
            Dictionary<string, List<ContainerInformation>> databases = await GetDatabasesInformationAsync().ConfigureAwait(true);
            SharedProperties.DatabaseCollection.LoadDatabases(databases);
        }

        public static async Task<dynamic> GetItemByIdAsync(string databaseName, string containerName, string itemId)
        { 
            return await CosmosExplorerCore.GetItemByIdAsync(databaseName, containerName, itemId).ConfigureAwait(true);
        }

        public static async Task LoadItemsAsync(string databaseName, string containerName)
        {
            // TODO: Validate id and partitionKey.
            SharedProperties.SelectedDatabase = databaseName;
            SharedProperties.SelectedContainer = containerName;

            SharedProperties.ItemListViewCollection.Clear();

            string query = "SELECT TOP 10 * FROM c";
            List<Tuple<string, string>> items = new List<Tuple<string, string>>();

            FeedIterator<dynamic> iterator = CosmosExplorerCore.GetQueryIterator(SharedProperties.SelectedDatabase, SharedProperties.SelectedContainer, query);

            while (iterator.HasMoreResults)
            {
                FeedResponse<dynamic> response = await iterator.ReadNextAsync().ConfigureAwait(true);
                foreach (var item in response)
                {
                    string partitionKey = SharedProperties.ContainerPartitionKey[SharedProperties.SelectedContainer].TrimStart('/');
                    items.Add(new Tuple<string, string>(item["id"].ToString(), item[partitionKey].ToString()));
                }
            }

            SharedProperties.ItemListViewCollection.LoadItems(items);
        }

        public static async Task SearchByQueryAsync(string query)
        {
            // TODO: Validate id and partitionKey.
            SharedProperties.ItemListViewCollection.Clear();

            List<Tuple<string, string>> items = new List<Tuple<string, string>>();

            FeedIterator<dynamic> iterator = CosmosExplorerCore.GetQueryIterator(SharedProperties.SelectedDatabase, SharedProperties.SelectedContainer, $"SELECT * FROM c {query}");

            while (iterator.HasMoreResults)
            {
                FeedResponse<dynamic> response = await iterator.ReadNextAsync().ConfigureAwait(true);
                foreach (var item in response)
                {
                    string partitionKey = SharedProperties.ContainerPartitionKey[SharedProperties.SelectedContainer].TrimStart('/');
                    items.Add(new Tuple<string, string>(item["id"].ToString(), item[partitionKey].ToString()));
                }
            }

            SharedProperties.ItemListViewCollection.LoadItems(items);
        }

        public static async Task DeleteItemAsync(string id, string partitionKey)
        {
            // TODO: Validate id and partitionKey.
            await CosmosExplorerCore.DeleteItemAsync(SharedProperties.SelectedDatabase, SharedProperties.SelectedContainer, id, partitionKey).ConfigureAwait(true);
        }
    }
}
