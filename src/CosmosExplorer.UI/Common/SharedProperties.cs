using System.Configuration;

namespace CosmosExplorer.UI.Common
{
    public static class SharedProperties
    {
        static SharedProperties()
        {
            Key = Convert.FromBase64String(ConfigurationManager.AppSettings["EncryptionKey"]);
            IV = Convert.FromBase64String(ConfigurationManager.AppSettings["EncryptionIV"]);

            UserSettingsFileName = ConfigurationManager.AppSettings["UserSettingsFile"] ?? "UserSettings.config";
        }

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

        public static byte[] Key { get; private set; }

        public static byte[] IV { get; private set; }

        public static string UserSettingsFileName { get; private set; }
    }
}
