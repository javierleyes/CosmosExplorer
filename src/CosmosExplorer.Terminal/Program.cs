// <copyright file="Program.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using CosmosExplorer.Core;
using Microsoft.Azure.Cosmos;

internal class Program
{
    // Windows
    // private const string CONNECTION_STRING = "AccountEndpoint=https://host.docker.internal:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

    // Linux
    // private const string CONNECTION_STRING = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

    private static async Task Main()
    {
        Console.Write("Enter the connection string: ");
        string connectionString = Console.ReadLine();

        // Create an instance of the CosmosExplorerHelper class.
        if (string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("Connection string cannot be null or empty.");
            return;
        }

        CosmosExplorerCore cosmosExplorerHelper = new CosmosExplorerCore(connectionString);

        while (true)
        {
            Console.WriteLine("Select an option:");
            Console.WriteLine("1) See all databases");
            Console.WriteLine("2) See all containers by a database");
            Console.WriteLine("3) Run a query by a database and a container");
            Console.WriteLine("4) Create or replace an item by a database, container and the item");
            Console.WriteLine("5) Delete an item by database, container, id and partitionKey");
            Console.WriteLine("6) Exit");
            string option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    await SeeAllDatabases(cosmosExplorerHelper).ConfigureAwait(true);
                    break;
                case "2":
                    await SeeAllContainersByDatabase(cosmosExplorerHelper).ConfigureAwait(true);
                    break;
                case "3":
                    await RunQueryByDatabaseAndContainer(cosmosExplorerHelper).ConfigureAwait(true);
                    break;
                case "4":
                    await CreateItemByDatabaseAndContainer(cosmosExplorerHelper).ConfigureAwait(true);
                    break;
                case "5":
                    await DeleteItemByDatabaseAndContainer(cosmosExplorerHelper).ConfigureAwait(true);
                    break;
                case "6":
                    return;
                default:
                    Console.WriteLine("Invalid option. Please try again.");
                    break;
            }
        }
    }

    private static async Task SeeAllDatabases(CosmosExplorerCore cosmosExplorerHelper)
    {
        FeedIterator<DatabaseProperties> iterator = cosmosExplorerHelper.GetDatabaseIterator();
        while (iterator.HasMoreResults)
        {
            FeedResponse<DatabaseProperties> databases = await iterator.ReadNextAsync().ConfigureAwait(true);
            foreach (var database in databases)
            {
                Console.WriteLine(database.Id);
            }
        }
    }

    private static async Task SeeAllContainersByDatabase(CosmosExplorerCore cosmosExplorerHelper)
    {
        Console.Write("Enter database name: ");
        string databaseName = Console.ReadLine();

        FeedIterator<ContainerProperties> containerIterator = cosmosExplorerHelper.GetContainerIterator(databaseName);
        while (containerIterator.HasMoreResults)
        {
            FeedResponse<ContainerProperties> containerResponse = await containerIterator.ReadNextAsync().ConfigureAwait(true);
            foreach (var container in containerResponse)
            {
                Console.WriteLine($"  Container: {container.Id}");
            }
        }
    }

    private static async Task RunQueryByDatabaseAndContainer(CosmosExplorerCore cosmosExplorerHelper)
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
            FeedResponse<dynamic> queryResponse = await queryIterator.ReadNextAsync().ConfigureAwait(true);
            foreach (var item in queryResponse)
            {
                Console.WriteLine(item);
            }
        }
    }

    private static async Task CreateItemByDatabaseAndContainer(CosmosExplorerCore cosmosExplorerHelper)
    {
        Console.Write("Enter database name: ");
        string databaseName = Console.ReadLine();

        Console.Write("Enter container name: ");
        string containerName = Console.ReadLine();

        Console.Write("Enter item (in JSON format): ");
        string itemJson = Console.ReadLine();

        Console.Write("Enter partition key: ");
        string partitionKey = Console.ReadLine();

        dynamic item = Newtonsoft.Json.JsonConvert.DeserializeObject(itemJson);

        dynamic createdItem = await cosmosExplorerHelper.UpsertItemAsync(databaseName, containerName, item, partitionKey);

        Console.WriteLine($"Created item: {createdItem}");
    }

    private static async Task DeleteItemByDatabaseAndContainer(CosmosExplorerCore cosmosExplorerHelper)
    {
        Console.Write("Enter database name: ");
        string databaseName = Console.ReadLine();

        Console.Write("Enter container name: ");
        string containerName = Console.ReadLine();

        Console.Write("Enter item id: ");
        string id = Console.ReadLine();

        Console.Write("Enter partition key: ");
        string partitionKey = Console.ReadLine();

        dynamic deletedItem = await cosmosExplorerHelper.DeleteItemAsync(databaseName, containerName, id, partitionKey).ConfigureAwait(true);

        Console.WriteLine($"Deleted item: {deletedItem}");
    }
}
