using CosmosExplorer.Core;
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
            SharedProperties.CosmosExplorerCore = new CosmosExplorerCore(connectionString);

            // Get the MainWindow instance
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.FilterPanel.IsEnabled = true;
                mainWindow.OutputTextBox.IsEnabled = true;
                mainWindow.LeftPanel.IsEnabled = true;
                mainWindow.Items.IsEnabled = true;
            }

            await SharedProperties.LoadDatabasesAsync().ConfigureAwait(true);

            // Close the modal
            this.Close();
        }
    }
}
