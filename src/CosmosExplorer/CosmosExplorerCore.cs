// <copyright file="CosmosExplorerCore.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace CosmosExplorer.Core
{
    using System.Dynamic;
    using Microsoft.Azure.Cosmos;
    using Newtonsoft.Json;

    /// <summary>
    /// Cosmos Explorer Core class to interact with Cosmos DB.
    /// </summary>
    public class CosmosExplorerCore
    {
        private readonly CosmosClient cosmosClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosExplorerCore"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public CosmosExplorerCore(string connectionString)
        {
            this.cosmosClient = this.CreateCosmosClient(connectionString);
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
            CosmosClientOptions options = new CosmosClientOptions()
            {
                HttpClientFactory = () => new HttpClient(new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator,
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
            return this.cosmosClient.GetDatabaseQueryIterator<DatabaseProperties>();
        }

        /// <summary>
        /// Gets an iterator to list the containers in a specific database.
        /// </summary>
        /// <param name="databaseName">The name of the database to list containers from.</param>
        /// <returns>A <see cref="FeedIterator{ContainerProperties}"/> to iterate through the containers.</returns>
        public FeedIterator<ContainerProperties> GetContainerIterator(string databaseName)
        {
            ValidateDatabaseName(databaseName);

            Database database = this.cosmosClient.GetDatabase(databaseName);
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
            ValidateDatabaseName(databaseName);
            ValidateContainerName(containerName);
            ValidateQuery(query);

            Container containerQuery = this.cosmosClient.GetContainer(databaseName, containerName);
            return containerQuery.GetItemQueryIterator<dynamic>(new QueryDefinition(query));
        }

        /// <summary>
        /// Retrieves an item from the specified container within the specified database using the item's id and partition key.
        /// </summary>
        /// <param name="databaseName">The name of the database containing the container.</param>
        /// <param name="containerName">The name of the container to retrieve the item from.</param>
        /// <param name="itemId">The ID of the item to retrieve.</param>
        /// <returns>A task representing the asynchronous operation, with a dynamic result containing the retrieved item.</returns>
        public async Task<dynamic> GetItemByIdAsync(string databaseName, string containerName, string itemId)
        {
            ValidateDatabaseName(databaseName);
            ValidateContainerName(containerName);
            ValidateItemId(itemId);

            string query = $"SELECT * FROM c WHERE c.id = '{itemId}'";
            FeedIterator<dynamic> queryIterator = this.GetQueryIterator(databaseName, containerName, query);

            while (queryIterator.HasMoreResults)
            {
                FeedResponse<dynamic> response = await queryIterator.ReadNextAsync().ConfigureAwait(false);
                foreach (var item in response)
                {
                    return item;
                }
            }

            return null;
        }

        /// <summary>
        /// Inserts a new item in the specified container within the specified database.
        /// </summary>
        /// <param name="databaseName">The name of the database containing the container.</param>
        /// <param name="containerName">The name of the container to insert the item into.</param>
        /// <param name="item">The item to insert.</param>
        /// <param name="partitionKey">The partition key for the item.</param>
        /// <returns>A task representing the asynchronous operation, with a dynamic result containing the inserted item.</returns>
        public async Task<dynamic> InsertItemAsync(string databaseName, string containerName, dynamic item, string partitionKey)
        {
            ValidateDatabaseName(databaseName);
            ValidateContainerName(containerName);
            ValidatePartitionKey(partitionKey);

            Container container = this.cosmosClient.GetContainer(databaseName, containerName);
            return await container.CreateItemAsync(item, new PartitionKey(partitionKey)).ConfigureAwait(false);
        }

        /// <summary>
        /// Upserts an item in the specified container within the specified database.
        /// If the item does not exist, it will be created. If it exists, it will be updated.
        /// </summary>
        /// <param name="databaseName">The name of the database containing the container.</param>
        /// <param name="containerName">The name of the container to upsert the item into.</param>
        /// <param name="item">The item to upsert.</param>
        /// <param name="partitionKey">The partition key for the item.</param>
        /// <returns>A task representing the asynchronous operation, with a dynamic result containing the upserted item.</returns>
        public async Task<dynamic> UpsertItemAsync(string databaseName, string containerName, dynamic item, string partitionKey)
        {
            Container container = this.cosmosClient.GetContainer(databaseName, containerName);
            return await container.UpsertItemAsync(item, new PartitionKey(partitionKey)).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates an item in the specified container within the specified database.
        /// </summary>
        /// <param name="databaseName">The name of the database containing the container.</param>
        /// <param name="containerName">The name of the container to update the item in.</param>
        /// <param name="itemId">The ID of the item to update.</param>
        /// <param name="partitionKey">The partition key for the item.</param>
        /// <param name="item">The updated item.</param>
        /// <returns>A task representing the asynchronous operation, with a dynamic result containing the updated item.</returns>
        public async Task<dynamic> UpdateItemAsync(string databaseName, string containerName, string itemId, string partitionKey, dynamic item)
        {
            ValidateDatabaseName(databaseName);
            ValidateContainerName(containerName);
            ValidateItemId(itemId);
            ValidatePartitionKey(partitionKey);

            Container container = this.cosmosClient.GetContainer(databaseName, containerName);

            item = FilterItemProperties(item);

            return await container.ReplaceItemAsync(item, itemId, new PartitionKey(partitionKey)).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes an item from the specified container within the specified database.
        /// </summary>
        /// <param name="databaseName">The name of the database containing the container.</param>
        /// <param name="containerName">The name of the container to delete the item from.</param>
        /// <param name="itemId">The ID of the item to delete.</param>
        /// <param name="partitionKey">The partition key for the item.</param>
        /// <returns>A task representing the asynchronous operation, with a dynamic result containing the deleted item.</returns>
        public async Task<dynamic> DeleteItemAsync(string databaseName, string containerName, string itemId, string partitionKey)
        {
            ValidateDatabaseName(databaseName);
            ValidateContainerName(containerName);
            ValidateItemId(itemId);
            ValidatePartitionKey(partitionKey);

            Container container = this.cosmosClient.GetContainer(databaseName, containerName);
            return await container.DeleteItemAsync<dynamic>(itemId, new PartitionKey(partitionKey)).ConfigureAwait(false);
        }

        /// <summary>
        /// Validates the database name.
        /// </summary>
        /// <param name="databaseName">The name of the database to validate.</param>
        private static void ValidateDatabaseName(string databaseName) => ValidateParameter(databaseName, nameof(databaseName));

        /// <summary>
        /// Validates the container name.
        /// </summary>
        /// <param name="containerName">The name of the container to validate.</param>
        private static void ValidateContainerName(string containerName) => ValidateParameter(containerName, nameof(containerName));

        /// <summary>
        /// Validates the query string.
        /// </summary>
        /// <param name="query">The query string to validate.</param>
        private static void ValidateQuery(string query) => ValidateParameter(query, nameof(query));

        /// <summary>
        /// Validates the item ID.
        /// </summary>
        /// <param name="itemId">The ID of the item to validate.</param>
        private static void ValidateItemId(string itemId) => ValidateParameter(itemId, nameof(itemId));

        /// <summary>
        /// Validates the partition key.
        /// </summary>
        /// <param name="partitionKey">The partition key to validate.</param>
        private static void ValidatePartitionKey(string partitionKey) => ValidateParameter(partitionKey, nameof(partitionKey));

        /// <summary>
        /// Validates a parameter.
        /// </summary>
        /// <param name="parameter">The parameter to validate.</param>
        /// <param name="paramName">The name of the parameter to validate.</param>
        /// <exception cref="ArgumentException">Thrown when the parameter is null or empty.</exception>
        private static void ValidateParameter(string parameter, string paramName)
        {
            if (string.IsNullOrWhiteSpace(parameter))
            {
                throw new ArgumentException($"{paramName} cannot be null or empty.", paramName);
            }
        }

        /// <summary>
        /// Filters out properties starting with "_" from the given item.
        /// </summary>
        /// <param name="item">The item to filter.</param>
        /// <returns>A new item without properties starting with "_".</returns>
        private static dynamic FilterItemProperties(dynamic item)
        {
            // Create a new object without properties starting with "_"
            ExpandoObject expandoObject = JsonConvert.DeserializeObject<ExpandoObject>(item);
            IDictionary<string, object> itemDictionary = expandoObject as IDictionary<string, object>;
            if (itemDictionary != null)
            {
                IDictionary<string, object> filteredItem = new ExpandoObject() as IDictionary<string, object>;
                foreach (KeyValuePair<string, object> kvp in itemDictionary)
                {
                    if (!kvp.Key.StartsWith("_"))
                    {
                        filteredItem.Add(kvp.Key, kvp.Value);
                    }
                }

                return filteredItem;
            }

            return expandoObject;
        }
    }
}
