namespace CosmosExplorer.UI.Common
{
    public static class SharedProperties
    {
        public static DatabaseTreeCollection DatabaseCollection { get; set; }

        public static ItemListViewCollection ItemListViewCollection { get; set; }

        public static Dictionary<string, string> ContainerPartitionKey { get; set; }

        public static LoaderIndicator LoaderIndicator { get; set; }

        public static string SelectedDatabase { get; set; }

        public static string SelectedContainer { get; set; }

        public static string SelectedItemId { get; set; }

        public static string SelectedItemPartitionKey { get; set; }
    }
}
