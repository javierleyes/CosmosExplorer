# Development environment

## Requirements
NET 8
Azure Cosmos DB emulator

### How to install Azure NoSQL
https://learn.microsoft.com/en-us/azure/cosmos-db/how-to-develop-emulator?tabs=docker-windows%2Ccsharp&pivots=api-nosql

Note: It's very important to use BASH.

1. docker pull mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest

2. docker run
--publish 8081:8081
--publish 10250-10255:10250-10255
--name linux-emulator
--detach
mcr.microsoft.com/cosmosdb/linux/azure-cosmos-emulator:latest

3. Go to https://localhost:8081/_explorer/index.html