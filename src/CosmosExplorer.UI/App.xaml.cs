using CosmosExplorer.Core;
using System.Configuration;
using System.Security.Cryptography;
using System.Windows;
using System.Xml.Linq;

namespace CosmosExplorer.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Generate and update the AES key and IV in App.config if they are empty
            UpdateEncryptionKeysInConfig();

            // Continue with the rest of the startup process
        }

        private void UpdateEncryptionKeysInConfig()
        {
            string configFilePath = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath;
            XDocument config = XDocument.Load(configFilePath);

            var appSettings = config.Element("configuration")?.Element("appSettings");
            if (appSettings == null)
            {
                throw new Exception("App.config is missing the appSettings section.");
            }

            var encryptionKeyElement = appSettings.Elements("add").FirstOrDefault(e => e.Attribute("key")?.Value == "EncryptionKey");
            var encryptionIVElement = appSettings.Elements("add").FirstOrDefault(e => e.Attribute("key")?.Value == "EncryptionIV");

            if (encryptionKeyElement == null || encryptionIVElement == null)
            {
                throw new Exception("App.config is missing the EncryptionKey or EncryptionIV settings.");
            }

            if (string.IsNullOrEmpty(encryptionKeyElement.Attribute("value")?.Value))
            {
                encryptionKeyElement.SetAttributeValue("value", Utils.GenerateRandomBase64String(32)); // 256 bits key (32 bytes)
            }

            if (string.IsNullOrEmpty(encryptionIVElement.Attribute("value")?.Value))
            {
                encryptionIVElement.SetAttributeValue("value", Utils.GenerateRandomBase64String(16)); // 128 bits IV (16 bytes)
            }

            config.Save(configFilePath);
        }
    }
}
