namespace CosmosExplorer.UI.Common
{
    public class ContainerInformation
    {
        public string Name { get; set; }

        public string Database { get; set; }

        public string PartitionKey { get; set; }

        public ContainerInformation(string name, string database, string partitionKey)
        {
            Name = name;
            Database = database;
            PartitionKey = partitionKey;
        }
    }
}
