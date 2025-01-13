namespace CosmosExplorer.UI.Common
{
    public static class SharedProperties
    {
        public static Dictionary<string, string> SavedConnections { get; set; } = new Dictionary<string, string>();

        public static DatabaseTreeCollection DatabaseCollection { get; set; }

        public static ItemListViewCollection ItemListViewCollection { get; set; }

        public static Dictionary<string, string> ContainerPartitionKey { get; set; } = new Dictionary<string, string>();

        public static LoaderIndicator LoaderIndicator { get; set; }

        public static bool IsEditMode { get; set; } = false;

        public static bool IsCreatingItem { get; set; } = false;

        public static string SelectedDatabase { get; set; }

        public static string SelectedContainer { get; set; }

        public static string SelectedItemId { get; set; }

        public static string SelectedItemPartitionKey { get; set; }

        public static string? SelectedItemJson { get; set; } = null;

        // TODO: Read from configuration.
        public static byte[] Key = Convert.FromBase64String("bWluZGVyLXNlY3JldC1rZXktZm9yLWFlcy1lbmNyeXB0aW9u");

        public static byte[] IV = Convert.FromBase64String("bWluZGVyLXNlY3JldC1pdi1mb3ItYWVz");
    }
}
