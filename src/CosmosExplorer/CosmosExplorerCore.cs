﻿// <copyright file="CosmosExplorerCore.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace CosmosExplorer.Core
{
    using Microsoft.Azure.Cosmos;

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
            Container containerQuery = this.cosmosClient.GetContainer(databaseName, containerName);
            return containerQuery.GetItemQueryIterator<dynamic>(new QueryDefinition(query));
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
        /// Deletes an item from the specified container within the specified database.
        /// </summary>
        /// <param name="databaseName">The name of the database containing the container.</param>
        /// <param name="containerName">The name of the container to delete the item from.</param>
        /// <param name="id">The ID of the item to delete.</param>
        /// <param name="partitionKey">The partition key for the item.</param>
        /// <returns>A task representing the asynchronous operation, with a dynamic result containing the deleted item.</returns>
        public async Task<dynamic> DeleteItemAsync(string databaseName, string containerName, string id, string partitionKey)
        {
            Container container = this.cosmosClient.GetContainer(databaseName, containerName);
            return await container.DeleteItemAsync<dynamic>(id, new PartitionKey(partitionKey)).ConfigureAwait(false);
        }
    }
}
