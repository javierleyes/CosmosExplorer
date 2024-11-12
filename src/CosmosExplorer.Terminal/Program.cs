using CosmosExplorer.Core;
using Microsoft.Azure.Cosmos;

class Program
{
    private const string CONNECTION_STRING = "AccountEndpoint=https://host.docker.internal:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
    //private const string CONNECTION_STRING = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

    static async Task Main()
    {
        // Create an instance of the CosmosExplorerHelper class.
        CosmosExplorerHelper cosmosExplorerHelper = new CosmosExplorerHelper(CONNECTION_STRING);

        while (true)
        {
            Console.WriteLine("Select an option:");
            Console.WriteLine("1) See all databases");
            Console.WriteLine("2) See all containers by a database");
            Console.WriteLine("3) Run a query by a database and a container");
            Console.WriteLine("4) Exit");
            string option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    await SeeAllDatabases(cosmosExplorerHelper);
                    break;
                case "2":
                    await SeeAllContainersByDatabase(cosmosExplorerHelper);
                    break;
                case "3":
                    await RunQueryByDatabaseAndContainer(cosmosExplorerHelper);
                    break;
                case "4":
                    return;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }

    private static async Task SeeAllDatabases(CosmosExplorerHelper cosmosExplorerHelper)
    {
        FeedIterator<DatabaseProperties> iterator = cosmosExplorerHelper.GetDatabaseIterator();
        while (iterator.HasMoreResults)
        {
            FeedResponse<DatabaseProperties> databases = await iterator.ReadNextAsync();
            foreach (var database in databases)
            {
                Console.WriteLine(database.Id);
            }
        }
    }

    private static async Task SeeAllContainersByDatabase(CosmosExplorerHelper cosmosExplorerHelper)
    {
        Console.Write("Enter database name: ");
        string databaseName = Console.ReadLine();

        FeedIterator<ContainerProperties> containerIterator = cosmosExplorerHelper.GetContainerIterator(databaseName);
        while (containerIterator.HasMoreResults)
        {
            FeedResponse<ContainerProperties> containerResponse = await containerIterator.ReadNextAsync();
            foreach (var container in containerResponse)
            {
                Console.WriteLine($"  Container: {container.Id}");
            }
        }
    }

    private static async Task RunQueryByDatabaseAndContainer(CosmosExplorerHelper cosmosExplorerHelper)
    {
        Console.Write("Enter database name: ");
        string databaseName = Console.ReadLine();
        Console.Write("Enter container name: ");
        string containerName = Console.ReadLine();
        Console.Write("Enter query: ");
        string query = Console.ReadLine();

        FeedIterator<dynamic> queryIterator = cosmosExplorerHelper.GetQueryIterator(databaseName, containerName, query);
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
