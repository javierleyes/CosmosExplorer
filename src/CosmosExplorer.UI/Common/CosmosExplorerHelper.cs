using CosmosExplorer.Core;
using Microsoft.Azure.Cosmos;
using System.Windows;

namespace CosmosExplorer.UI.Common
{
    public static class CosmosExplorerHelper
    {
        private static CosmosExplorerCore CosmosExplorerCore { get; set; }

        public static void Initialize(string connectionString)
        {
            try
            {
                CosmosExplorerCore = new CosmosExplorerCore(connectionString);
            }
            catch (InvalidOperationException ex)
            {
                throw new Exception($"Error initializing CosmosExplorerCore: {ex.Message}");
            }
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
                MessageBox.Show("An error occurred while retrieving databases.", "Get databases error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("An error occurred while retrieving database information.", "Retrieving database information", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("An error occurred while loading databases.", "Load databases", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("An error occurred while retrieving item.", "Retrieving item", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("An error occurred while loading items.", "Loading items", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("An error occurred while searching by query.", "Searching by query", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("An error occurred while deleting item.", "Deleting item", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("An error occurred while saving item.", "Saving item", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("An error occurred while updating item.", "Updating item", MessageBoxButton.OK, MessageBoxImage.Error);
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
