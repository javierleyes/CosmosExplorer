using System.Configuration;
using System.IO;

namespace CosmosExplorer.UI.Common
{
    public static class SharedProperties
    {
        static SharedProperties()
        {
            string cosmosExplorerConfigFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CosmosExplorer", ConfigurationManager.AppSettings["CosmosExplorerConfig"] ?? "CosmosExplorer.config");
            if (File.Exists(cosmosExplorerConfigFilePath))
            {
                var config = ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap { ExeConfigFilename = cosmosExplorerConfigFilePath }, ConfigurationUserLevel.None);
                Key = Convert.FromBase64String(config.AppSettings.Settings["EncryptionKey"].Value);
                IV = Convert.FromBase64String(config.AppSettings.Settings["EncryptionIV"].Value);
            }
            else
            {
                throw new Exception("CosmosExplorer.config is missing.");
            }

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
