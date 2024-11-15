using CosmosExplorer.Core;
using Microsoft.Azure.Cosmos;
using System.Windows;
using System.Windows.Controls;

namespace CosmosExplorer.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CosmosExplorerCore _cosmosExplorerHelper;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            string connectionString = ConnectionStringTextBox.Text;
            _cosmosExplorerHelper = new CosmosExplorerCore(connectionString);
            OutputTextBox.Text = "Connected to Cosmos DB.";
        }

        private async void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_cosmosExplorerHelper == null)
            {
                OutputTextBox.Text = "Please connect to Cosmos DB first.";
                return;
            }

            string selectedOption = ((ComboBoxItem)OptionsComboBox.SelectedItem).Content.ToString();
            switch (selectedOption)
            {
                case "See all databases":
                    await SeeAllDatabases().ConfigureAwait(true);
                    break;
                case "See all containers by a database":
                    await SeeAllContainersByDatabase().ConfigureAwait(true);
                    break;
                case "Run a query by a database and a container":
                    await RunQueryByDatabaseAndContainer().ConfigureAwait(true);
                    break;
                case "Create or replace an item by a database, container and the item":
                    await CreateItemByDatabaseAndContainer().ConfigureAwait(true);
                    break;
                case "Delete an item by database, container, id and partitionKey":
                    await DeleteItemByDatabaseAndContainer().ConfigureAwait(true);
                    break;
                default:
                    OutputTextBox.Text = "Invalid option selected.";
                    break;
            }
        }

        private async Task SeeAllDatabases()
        {
            FeedIterator<DatabaseProperties> iterator = _cosmosExplorerHelper.GetDatabaseIterator();
            while (iterator.HasMoreResults)
            {
                FeedResponse<DatabaseProperties> databases = await iterator.ReadNextAsync().ConfigureAwait(true);
                foreach (var database in databases)
                {
                    OutputTextBox.Text += database.Id + Environment.NewLine;
                }
            }
        }

        private async Task SeeAllContainersByDatabase()
        {
            // Implement similar to SeeAllDatabases
        }

        private async Task RunQueryByDatabaseAndContainer()
        {
            // Implement similar to SeeAllDatabases
        }

        private async Task CreateItemByDatabaseAndContainer()
        {
            // Implement similar to SeeAllDatabases
        }

        private async Task DeleteItemByDatabaseAndContainer()
        {
            // Implement similar to SeeAllDatabases
        }
    }
}