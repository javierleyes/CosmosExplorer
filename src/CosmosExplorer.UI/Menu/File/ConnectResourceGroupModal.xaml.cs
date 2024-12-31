using CosmosExplorer.UI.Common;
using System.Windows;

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

                CosmosExplorerHelper.Initialize(connectionString);

                SharedProperties.LoaderIndicator.SetLoaderIndicator(true);
                ConnectionStringPanel.Visibility = Visibility.Collapsed;
                Loader.Visibility = Visibility.Visible;

                // Get the MainWindow instance
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.LeftPanel.IsEnabled = true;
                }

                await CosmosExplorerHelper.LoadDatabasesAsync().ConfigureAwait(true);

                // Close the modal
                this.Close();
            }
            catch (Exception ex)
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
                SharedProperties.SavedConnections.Add(modal.ConnectionStringNameTextBox.Text, connectionString);
            }
        }
    }
}
