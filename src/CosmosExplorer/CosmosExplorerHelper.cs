using Microsoft.Azure.Cosmos;

namespace CosmosExplorer.Core
{
    public class CosmosExplorerHelper
    {
        private readonly CosmosClient _cosmosClient;

        public CosmosExplorerHelper(string connectionString)
        {
            _cosmosClient = CreateCosmosClient(connectionString);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CosmosClient"/> class with the specified connection string.
        /// This method configures the client to accept self-signed certificates in development environments.
        /// </summary>
        /// <param name="connectionString">The connection string to use for the Cosmos DB account.</param>
        /// <returns>A new instance of the <see cref="CosmosClient"/> class.</returns>
        public CosmosClient CreateCosmosClient(string connectionString)
        {
            // This is a workaround to accept self-signed certificates in development.
            CosmosClientOptions options = new()
            {
                HttpClientFactory = () => new HttpClient(new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                }),
                ConnectionMode = ConnectionMode.Gateway,
            };

            return new CosmosClient(connectionString, clientOptions: options);
        }

        /// <summary>
        /// Gets an iterator to list the databases in the Cosmos DB account.
        /// </summary>
        /// <returns>A <see cref="FeedIterator{DatabaseProperties}"/> to iterate through the databases.</returns>
        public FeedIterator<DatabaseProperties> GetDatabaseIterator()
        {
            return _cosmosClient.GetDatabaseQueryIterator<DatabaseProperties>();
        }

        /// <summary>
        /// Gets an iterator to list the containers in a specific database.
        /// </summary>
        /// <param name="databaseName">The name of the database to list containers from.</param>
        /// <returns>A <see cref="FeedIterator{ContainerProperties}"/> to iterate through the containers.</returns>
        public FeedIterator<ContainerProperties> GetContainerIterator(string databaseName)
        {
            Database database = _cosmosClient.GetDatabase(databaseName);
            return database.GetContainerQueryIterator<ContainerProperties>();
        }

        /// <summary>
        /// Gets an iterator to run a query in a specific container.
        /// </summary>
        /// <param name="databaseName">The name of the database containing the container.</param>
        /// <param name="containerName">The name of the container to run the query against.</param>
        /// <param name="query">The query to execute.</param>
        /// <returns>A <see cref="FeedIterator{T}"/> to iterate through the query results.</returns>
        public FeedIterator<dynamic> GetQueryIterator(string databaseName, string containerName, string query)
        {
            Container containerQuery = _cosmosClient.GetContainer(databaseName, containerName);
            return containerQuery.GetItemQueryIterator<dynamic>(new QueryDefinition(query));
        }
    }
}
