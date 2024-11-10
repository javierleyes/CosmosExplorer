using Microsoft.Azure.Cosmos;

class Program
{
    private const string ConnectionString = "AccountEndpoint=https://host.docker.internal:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

    static async Task Main(string[] args)
    {
        // Create Cosmos Client.
        CosmosClient cosmosClient = CreateCosmosClient();

        // Create an iterator to list the databases.
        FeedIterator<DatabaseProperties> iterator = GetDatabaseIterator(cosmosClient);

        // TODO: Remove this variable, just for test.
        string databaseName = string.Empty;

        // Iterate through the databases and print their names.
        while (iterator.HasMoreResults)
        {
            FeedResponse<DatabaseProperties> response = await iterator.ReadNextAsync();
            foreach (var database in response)
            {
                databaseName = database.Id;
                Console.WriteLine(database.Id);
            }
        }

        // Create an iterator to list the containers in the current database.
        FeedIterator<ContainerProperties> containerIterator = GetContainerIterator(cosmosClient, databaseName);

        // Iterate through the containers and print their names.
        while (containerIterator.HasMoreResults)
        {
            FeedResponse<ContainerProperties> containerResponse = await containerIterator.ReadNextAsync();
            foreach (var container in containerResponse)
            {
                Console.WriteLine($"  Container: {container.Id}");
            }
        }

        // Run a query in a specific container.
        databaseName = "Oversight";
        string containerName = "Project"; 
        Container containerQuery = cosmosClient.GetContainer(databaseName, containerName);

        // Define the query
        string query = "SELECT * FROM c WHERE c.ProjectId = '45345345asdfs'"; // Replace with your query
        FeedIterator<dynamic> queryIterator = containerQuery.GetItemQueryIterator<dynamic>(new QueryDefinition(query));

        // Iterate through the query results and print them
        while (queryIterator.HasMoreResults)
        {
            FeedResponse<dynamic> queryResponse = await queryIterator.ReadNextAsync();
            foreach (var item in queryResponse)
            {
                Console.WriteLine(item);
            }
        }
    }

    /// <summary>
    /// Creates a new instance of the <see cref="CosmosClient"/> class with custom options.
    /// This method configures the client to accept self-signed certificates in development.
    /// </summary>
    /// <returns>A configured <see cref="CosmosClient"/> instance.</returns>
    private static CosmosClient CreateCosmosClient()
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

        return new CosmosClient(ConnectionString, clientOptions: options);
    }

    /// <summary>
    /// Gets an iterator to list the databases in the Cosmos DB account.
    /// </summary>
    /// <param name="cosmosClient">The <see cref="CosmosClient"/> instance to use for querying databases.</param>
    /// <returns>A <see cref="FeedIterator{DatabaseProperties}"/> to iterate through the databases.</returns>
    private static FeedIterator<DatabaseProperties> GetDatabaseIterator(CosmosClient cosmosClient)
    {
        return cosmosClient.GetDatabaseQueryIterator<DatabaseProperties>();
    }

    /// <summary>
    /// Gets an iterator to list the containers in a specific database.
    /// </summary>
    /// <param name="cosmosClient">The <see cref="CosmosClient"/> instance to use for querying containers.</param>
    /// <param name="databaseName">The name of the database to list containers from.</param>
    /// <returns>A <see cref="FeedIterator{ContainerProperties}"/> to iterate through the containers.</returns>
    private static FeedIterator<ContainerProperties> GetContainerIterator(CosmosClient cosmosClient, string databaseName)
    {
        Database database = cosmosClient.GetDatabase(databaseName);
        return database.GetContainerQueryIterator<ContainerProperties>();
    }

    /// <summary>
    /// Gets an iterator to run a query in a specific container.
    /// </summary>
    /// <param name="cosmosClient">The <see cref="CosmosClient"/> instance to use for querying items.</param>
    /// <param name="databaseName">The name of the database containing the container.</param>
    /// <param name="containerName">The name of the container to run the query against.</param>
    /// <param name="query">The query to execute.</param>
    /// <returns>A <see cref="FeedIterator{T}"/> to iterate through the query results.</returns>
    private static FeedIterator<dynamic> GetQueryIterator(CosmosClient cosmosClient, string databaseName, string containerName, string query)
    {
        Container containerQuery = cosmosClient.GetContainer(databaseName, containerName);
        return containerQuery.GetItemQueryIterator<dynamic>(new QueryDefinition(query));
    }
}
