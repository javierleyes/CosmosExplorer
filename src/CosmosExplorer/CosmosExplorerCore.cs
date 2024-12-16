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
        /// Creates a new instance of the CosmosClient with the specified connection string.
        /// </summary>
        /// <param name="connectionString">The connection string to connect to the Cosmos DB account.</param>
        /// <returns>A new instance of the <see cref="CosmosClient"/> class.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the CosmosClient creation fails.</exception>
        public CosmosClient CreateCosmosClient(string connectionString)
        {
            try
            {
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
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create CosmosClient.", ex);
            }
        }

        /// <summary>
        /// Gets an iterator to list the databases in the Cosmos DB account.
        /// </summary>
        /// <returns>A <see cref="FeedIterator{DatabaseProperties}"/> to iterate through the databases.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the database iterator retrieval fails.</exception>
        public FeedIterator<DatabaseProperties> GetDatabaseIterator()
        {
            try
            {
                return this.cosmosClient.GetDatabaseQueryIterator<DatabaseProperties>();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to get database iterator.", ex);
            }
        }

        /// <summary>
        /// Gets an iterator to list the containers in the specified database.
        /// </summary>
        /// <param name="databaseName">The name of the database containing the containers.</param>
        /// <returns>A <see cref="FeedIterator{ContainerProperties}"/> to iterate through the containers.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the container iterator retrieval fails.</exception>
        public FeedIterator<ContainerProperties> GetContainerIterator(string databaseName)
        {
            try
            {
                ValidateDatabaseName(databaseName);
                Database database = this.cosmosClient.GetDatabase(databaseName);
                return database.GetContainerQueryIterator<ContainerProperties>();
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new InvalidOperationException($"Failed to get container iterator for database '{databaseName}'.", ex);
            }
        }

        /// <summary>
        /// Gets an iterator to list the results of a query in the specified container within the specified database.
        /// </summary>
        /// <param name="databaseName">The name of the database containing the container.</param>
        /// <param name="containerName">The name of the container to query.</param>
        /// <param name="query">The query to execute.</param>
        /// <returns>A <see cref="FeedIterator{T}"/> to iterate through the query results.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the query iterator retrieval fails.</exception>
        public FeedIterator<dynamic> GetQueryIterator(string databaseName, string containerName, string query)
        {
            try
            {
                ValidateDatabaseName(databaseName);
                ValidateContainerName(containerName);
                ValidateQuery(query);

                Container containerQuery = this.cosmosClient.GetContainer(databaseName, containerName);
                return containerQuery.GetItemQueryIterator<dynamic>(new QueryDefinition(query));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get query iterator for container '{containerName}' in database '{databaseName}'.", ex);
            }
        }

        /// <summary>
        /// Gets an item by its ID from the specified container within the specified database.
        /// </summary>
        /// <param name="databaseName">The name of the database containing the container.</param>
        /// <param name="containerName">The name of the container containing the item.</param>
        /// <param name="itemId">The ID of the item to retrieve.</param>
        /// <returns>A task representing the asynchronous operation, with a dynamic result containing the retrieved item.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the item retrieval fails.</exception>
        public async Task<dynamic> GetItemByIdAsync(string databaseName, string containerName, string itemId)
        {
            try
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
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to get item by ID '{itemId}' from container '{containerName}' in database '{databaseName}'.", ex);
            }
        }

        /// <summary>
        /// Inserts an item into the specified container within the specified database.
        /// </summary>
        /// <param name="databaseName">The name of the database containing the container.</param>
        /// <param name="containerName">The name of the container to insert the item into.</param>
        /// <param name="item">The item to insert.</param>
        /// <param name="partitionKey">The partition key for the item.</param>
        /// <returns>A task representing the asynchronous operation, with a dynamic result containing the inserted item.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the item insertion fails.</exception>
        public async Task<dynamic> InsertItemAsync(string databaseName, string containerName, dynamic item, string partitionKey)
        {
            try
            {
                ValidateDatabaseName(databaseName);
                ValidateContainerName(containerName);
                ValidatePartitionKey(partitionKey);

                Container container = this.cosmosClient.GetContainer(databaseName, containerName);
                return await container.CreateItemAsync(item, new PartitionKey(partitionKey)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to insert item into container '{containerName}' in database '{databaseName}'.", ex);
            }
        }

        /// <summary>
        /// Updates an item in the specified container within the specified database.
        /// </summary>
        /// <param name="databaseName">The name of the database containing the container.</param>
        /// <param name="containerName">The name of the container containing the item.</param>
        /// <param name="itemId">The ID of the item to update.</param>
        /// <param name="partitionKey">The partition key for the item.</param>
        /// <param name="item">The item to update.</param>
        /// <returns>A task representing the asynchronous operation, with a dynamic result containing the updated item.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the item update fails.</exception>
        public async Task<dynamic> UpdateItemAsync(string databaseName, string containerName, string itemId, string partitionKey, dynamic item)
        {
            try
            {
                ValidateDatabaseName(databaseName);
                ValidateContainerName(containerName);
                ValidateItemId(itemId);
                ValidatePartitionKey(partitionKey);

                Container container = this.cosmosClient.GetContainer(databaseName, containerName);

                item = FilterItemProperties(item);

                return await container.ReplaceItemAsync(item, itemId, new PartitionKey(partitionKey)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update item with ID '{itemId}' in container '{containerName}' in database '{databaseName}'.", ex);
            }
        }

        /// <summary>
        /// Deletes an item from the specified container within the specified database.
        /// </summary>
        /// <param name="databaseName">The name of the database containing the container.</param>
        /// <param name="containerName">The name of the container containing the item.</param>
        /// <param name="itemId">The ID of the item to delete.</param>
        /// <param name="partitionKey">The partition key for the item.</param>
        /// <returns>A task representing the asynchronous operation, with a dynamic result containing the deleted item.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the item deletion fails.</exception>
        public async Task<dynamic> DeleteItemAsync(string databaseName, string containerName, string itemId, string partitionKey)
        {
            try
            {
                ValidateDatabaseName(databaseName);
                ValidateContainerName(containerName);
                ValidateItemId(itemId);
                ValidatePartitionKey(partitionKey);

                Container container = this.cosmosClient.GetContainer(databaseName, containerName);
                return await container.DeleteItemAsync<dynamic>(itemId, new PartitionKey(partitionKey)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to delete item with ID '{itemId}' from container '{containerName}' in database '{databaseName}'.", ex);
            }
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
