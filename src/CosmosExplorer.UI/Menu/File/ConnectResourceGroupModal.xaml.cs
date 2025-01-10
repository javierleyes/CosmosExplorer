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
            // TODO: avoid saving the same connection string multiple times.

            string connectionString = ConnectionStringTextBox.Text;

            if (string.IsNullOrEmpty(connectionString))
            {
                MessageBox.Show("The connection string can not be empty", "Save connection string", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SaveConnectionStringModal modal = new SaveConnectionStringModal();

            if (modal.ShowDialog() is true)
            {
                SharedProperties.SavedConnections.Add(modal.ConnectionStringNameTextBox.Text, connectionString);

                // TODO: Save the file's name in the app settings.
                string exeDirectory = AppContext.BaseDirectory;
                string filePath = Path.Combine(exeDirectory, "savedConnections.json");

                if (!File.Exists(filePath))
                {
                    return;
                }

                string jsonString = JsonSerializer.Serialize(SharedProperties.SavedConnections, new JsonSerializerOptions { WriteIndented = true });

                File.WriteAllText(filePath, jsonString);

                if (Application.Current.MainWindow is MainWindow mainWindowInstance)
                {
                    MenuItem menuItem = new MenuItem();
                    menuItem.Header = modal.ConnectionStringNameTextBox.Text;
                    menuItem.Click += ConnectionMenuItem_Click;

                    mainWindowInstance.SavedConnectionMenuItem.Items.Add(menuItem);
                }
            }
        }

        private async void ConnectionMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindowInstance)
            {
                SharedProperties.LoaderIndicator.SetLoaderIndicator(true);
                mainWindowInstance.MainPanel.Visibility = Visibility.Collapsed;

                if (!SharedProperties.SavedConnections.TryGetValue((sender as MenuItem)?.Header.ToString(), out string connectionString))
                {
                    MessageBox.Show("The saved connection value is invalid.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                CosmosExplorerHelper.Initialize(connectionString);

                await CosmosExplorerHelper.LoadDatabasesAsync().ConfigureAwait(true);

                mainWindowInstance.MainPanel.Visibility = Visibility.Visible;
                SharedProperties.LoaderIndicator.SetLoaderIndicator(false);
                mainWindowInstance.LeftPanel.IsEnabled = true;
            }
        }
    }
}
