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
            string connectionString = ConnectionStringTextBox.Text;
            CosmosExplorerHelper.Initialize(connectionString);

            SharedProperties.LoaderIndicator.SetLoaderIndicator(true);

            // Get the MainWindow instance
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.FilterPanel.IsEnabled = true;
                mainWindow.ItemDescriptionTextBox.IsEnabled = true;
                mainWindow.LeftPanel.IsEnabled = true;
                mainWindow.Items.IsEnabled = true;
            }

            await CosmosExplorerHelper.LoadDatabasesAsync().ConfigureAwait(true);

            // Close the modal
            this.Close();

            SharedProperties.LoaderIndicator.SetLoaderIndicator(false);
        }
    }
}
