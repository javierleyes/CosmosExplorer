using CosmosExplorer.Core;
using Microsoft.Azure.Cosmos;

class Program
{
    private const string CONNECTION_STRING = "AccountEndpoint=https://host.docker.internal:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

    static async Task Main(string[] args)
    {
        // Create a new instance of the CosmosClient class.
        CosmosExplorerHelper cosmosExplorerHelper = new CosmosExplorerHelper(CONNECTION_STRING);

        // Create an iterator to list the databases.
        FeedIterator<DatabaseProperties> iterator = cosmosExplorerHelper.GetDatabaseIterator();

        // TODO: Remove this variable, just for test.
        string databaseName = string.Empty;

        // Iterate through the databases and print their names.
        while (iterator.HasMoreResults)
        {
            FeedResponse<DatabaseProperties> databases = await iterator.ReadNextAsync();
            foreach (var database in databases)
            {
                databaseName = database.Id;
                Console.WriteLine(database.Id);
            }
        }

        // Create an iterator to list the containers in the current database.
        FeedIterator<ContainerProperties> containerIterator = cosmosExplorerHelper.GetContainerIterator(databaseName);

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

        // Define the query
        string query = "SELECT * FROM c WHERE c.ProjectId = '45345345asdfs'"; // Replace with your query

        FeedIterator<dynamic> queryIterator = cosmosExplorerHelper.GetQueryIterator(databaseName, containerName, query);

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
}
