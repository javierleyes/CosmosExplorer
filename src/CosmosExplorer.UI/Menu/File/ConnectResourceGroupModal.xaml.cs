using CosmosExplorer.Core;
using CosmosExplorer.UI.Common;
using System;
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

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = ConnectionStringTextBox.Text;
            SharedProperties.CosmosExplorerCore = new CosmosExplorerCore(connectionString);

            // Get the MainWindow instance
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.OutputTextBox.Text = "Connected to Cosmos DB.";
                mainWindow.Actions.IsEnabled = true;
                mainWindow.OutputTextBox.IsEnabled = true;
            }

            // Close the modal
            this.Close();
        }
    }
}
