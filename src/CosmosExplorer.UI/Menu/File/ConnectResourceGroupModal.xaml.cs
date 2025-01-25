using CosmosExplorer.Core;
using CosmosExplorer.UI.Common;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace CosmosExplorer.UI
{
    /// <summary>
    /// Interaction logic for ConnectResourceGroupModal.xaml
    /// </summary>
    public partial class ConnectResourceGroupModal : Window
    {
        public ConnectResourceGroupModal()
        {
            InitializeComponent();
        }

        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string connectionString = ConnectionStringTextBox.Text;

                if (string.IsNullOrEmpty(connectionString))
                {
                    MessageBox.Show("The connection string can not be empty", "Save connection string", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                this.Close();

                SharedProperties.LoaderIndicator.SetLoaderIndicator(true);

                // Get the MainWindow instance
                if (Application.Current.MainWindow is MainWindow mainWindowInstance1)
                {
                    mainWindowInstance1.MainPanel.Visibility = Visibility.Collapsed;
                }

                CosmosExplorerHelper.Initialize(connectionString);

                await CosmosExplorerHelper.LoadDatabasesAsync().ConfigureAwait(true);

                // Get the MainWindow instance
                if (Application.Current.MainWindow is MainWindow mainWindowInstance2)
                {
                    mainWindowInstance2.MainPanel.Visibility = Visibility.Visible;
                    mainWindowInstance2.LeftPanel.IsEnabled = true;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("An error occurred while connecting to the resource group. Please check your connection details and try again.", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SharedProperties.LoaderIndicator.SetLoaderIndicator(false);
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = ConnectionStringTextBox.Text;

            if (string.IsNullOrEmpty(connectionString))
            {
                MessageBox.Show("The connection string can not be empty", "Save connection string", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SaveConnectionStringModal modal = new SaveConnectionStringModal();

            if (modal.ShowDialog() is true)
            {
                string connectionStringName = modal.ConnectionStringNameTextBox.Text;

                // Check if the key already exists
                if (SharedProperties.SavedConnections.ContainsKey(connectionStringName))
                {
                    MessageBox.Show("This connection string is already saved.", "Save connection string", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                SharedProperties.SavedConnections.Add(connectionStringName, connectionString);

                string userSettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CosmosExplorer", SharedProperties.UserSettingsFileName);

                string savedConnections = JsonSerializer.Serialize(SharedProperties.SavedConnections, new JsonSerializerOptions { WriteIndented = true });

                string encryptedSavedConnections = Utils.Encrypt(savedConnections, SharedProperties.Key, SharedProperties.IV);

                File.WriteAllText(userSettingsPath, encryptedSavedConnections);

                if (Application.Current.MainWindow is MainWindow mainWindowInstance)
                {
                    MenuItem menuItem = new MenuItem();
                    menuItem.Header = connectionStringName;
                    menuItem.Click += ConnectionMenuItem_Click;

                    mainWindowInstance.SavedConnectionMenuItem.Items.Add(menuItem);
                    mainWindowInstance.SavedConnectionMenuItem.IsEnabled = true;
                }
            }
        }

        private async void ConnectionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindowInstance)
            {
                mainWindowInstance.Title = "Cosmos Explorer";

                SharedProperties.LoaderIndicator.SetLoaderIndicator(true);
                mainWindowInstance.MainPanel.Visibility = Visibility.Collapsed;

                if (!SharedProperties.SavedConnections.TryGetValue((sender as MenuItem)?.Header.ToString(), out string connectionString))
                {
                    MessageBox.Show("The saved connection value is invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                CosmosExplorerHelper.Initialize(connectionString);

                await CosmosExplorerHelper.LoadDatabasesAsync().ConfigureAwait(true);

                mainWindowInstance.Title = $"{mainWindowInstance.Title} - {(e.Source as MenuItem)?.Header}";
                mainWindowInstance.MainPanel.Visibility = Visibility.Visible;
                SharedProperties.LoaderIndicator.SetLoaderIndicator(false);
                mainWindowInstance.LeftPanel.IsEnabled = true;
            }
        }
    }
}
