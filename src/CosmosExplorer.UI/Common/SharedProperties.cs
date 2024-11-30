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
    }
}
