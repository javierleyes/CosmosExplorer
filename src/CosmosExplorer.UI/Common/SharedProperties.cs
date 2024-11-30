using CosmosExplorer.Core;
using Microsoft.Azure.Cosmos;

namespace CosmosExplorer.UI.Common
{
    public static class SharedProperties
    {
        public static CosmosExplorerCore CosmosExplorerCore { get; set; }

        public static DatabaseTreeCollection DatabaseCollection { get; set; }

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

        public static async Task<Dictionary<string, List<string>>> GetDatabasesInformationAsync()
        {
            Dictionary<string, List<string>> databases = new Dictionary<string, List<string>>();

            List<string> databaseNames = await SharedProperties.GetDatabases().ConfigureAwait(true);
            foreach (var database in databaseNames)
            {
                List<string> containers = new List<string>();

                FeedIterator<ContainerProperties> containerIterator = SharedProperties.CosmosExplorerCore.GetContainerIterator(database);
                while (containerIterator.HasMoreResults)
                {
                    FeedResponse<ContainerProperties> containerResponse = await containerIterator.ReadNextAsync().ConfigureAwait(true);
                    foreach (var container in containerResponse)
                    {
                        containers.Add(container.Id);
                    }
                }

                databases.Add(database, containers);
            }

            return databases;
        }

        public static async Task LoadDatabasesAsync()
        {
            Dictionary<string, List<string>> databases = await GetDatabasesInformationAsync().ConfigureAwait(true);
            DatabaseCollection.LoadDatabases(databases);
        }
    }
}
