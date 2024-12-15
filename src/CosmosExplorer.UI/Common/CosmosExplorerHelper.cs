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

            try
            {
                FeedIterator<DatabaseProperties> iterator = CosmosExplorerCore.GetDatabaseIterator();

                while (iterator.HasMoreResults)
                {
                    FeedResponse<DatabaseProperties> databases = await iterator.ReadNextAsync().ConfigureAwait(true);
                    foreach (var database in databases)
                    {
                        databaseNames.Add(database.Id);
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Error retrieving databases: {ex.Message}");
            }

            return databaseNames;
        }

        public static async Task<Dictionary<string, List<ContainerInformation>>> GetDatabasesInformationAsync()
        {
            Dictionary<string, List<ContainerInformation>> databases = new Dictionary<string, List<ContainerInformation>>();
            SharedProperties.ContainerPartitionKey = new Dictionary<string, string>();

            try
            {
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
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Error retrieving database information: {ex.Message}");
            }

            return databases;
        }

        public static async Task LoadDatabasesAsync()
        {
            try
            {
                Dictionary<string, List<ContainerInformation>> databases = await GetDatabasesInformationAsync().ConfigureAwait(true);
                SharedProperties.DatabaseCollection.LoadDatabases(databases);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Error loading databases: {ex.Message}");
            }
        }

        public static async Task<dynamic> GetItemByIdAsync(string databaseName, string containerName, string itemId)
        {
            try
            {
                return await CosmosExplorerCore.GetItemByIdAsync(databaseName, containerName, itemId).ConfigureAwait(true);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Error retrieving item by ID: {ex.Message}");
                return null;
            }
        }

        public static async Task LoadItemsAsync(string databaseName, string containerName)
        {
            try
            {
                SharedProperties.SelectedDatabase = databaseName;
                SharedProperties.SelectedContainer = containerName;

                SharedProperties.ItemListViewCollection.Clear();

                string query = "SELECT TOP 30 * FROM c";
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
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Error loading items: {ex.Message}");
            }
        }

        public static async Task SearchByQueryAsync(string query)
        {
            try
            {
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
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Error searching by query: {ex.Message}");
            }
        }

        public static async Task DeleteItemAsync(string id, string partitionKey)
        {
            try
            {
                await CosmosExplorerCore.DeleteItemAsync(SharedProperties.SelectedDatabase, SharedProperties.SelectedContainer, id, partitionKey).ConfigureAwait(true);

                SharedProperties.SelectedItemId = string.Empty;
                SharedProperties.SelectedItemPartitionKey = string.Empty;
                SharedProperties.SelectedItemJson = string.Empty;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Error deleting item: {ex.Message}");
            }
        }

        public static async Task SaveItemAsync(dynamic item, string partitionKey)
        {
            try
            {
                await CosmosExplorerCore.InsertItemAsync(SharedProperties.SelectedDatabase, SharedProperties.SelectedContainer, item, partitionKey).ConfigureAwait(true);
            }
            catch (InvalidOperationException ex)
            {
                // Log the exception or handle it as needed
                Console.WriteLine($"Error saving item: {ex.Message}");
            }
        }

        public static async Task UpdateItemAsync(string itemId, string partitionKey, dynamic item)
        {
            try
            {
                await CosmosExplorerCore.UpdateItemAsync(SharedProperties.SelectedDatabase, SharedProperties.SelectedContainer, itemId, partitionKey, item).ConfigureAwait(true);
            }
            catch (InvalidOperationException ex)
            {
                // Log the exception or handle it as needed
                Console.WriteLine($"Error updating item: {ex.Message}");
            }
        }

        public static void SetEditMode(string original, string current)
        {
            string originalHash = GenerateHash(original);
            string currentHash = GenerateHash(current);

            SharedProperties.IsEditMode = !originalHash.Equals(currentHash, StringComparison.Ordinal);
        }

        private static string GenerateHash(string input)
        {
            return Utils.GenerateHash(input);
        }
    }
}
