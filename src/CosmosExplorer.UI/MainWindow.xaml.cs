using CosmosExplorer.UI.Common;
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
        public MainWindow()
        {
            InitializeComponent();
            SharedProperties.DatabaseCollection = new DatabaseTreeCollection();
            DatabaseTreeView.ItemsSource = SharedProperties.DatabaseCollection;
        }

        private void OpenConnectionModal_Click(object sender, RoutedEventArgs e)
        {
            ConnectResourceGroupModal modal = new ConnectResourceGroupModal();
            modal.Owner = this;
            modal.ShowDialog();
        }

        private void CloseApplication_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }

        private async void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            if (SharedProperties.CosmosExplorerCore is null)
            {
                OutputTextBox.Text = "Please connect to Cosmos DB first.";
                return;
            }

            string selectedOption = ((ComboBoxItem)OptionsComboBox.SelectedItem).Content.ToString();
            switch (selectedOption)
            {
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

        private async Task SeeAllContainersByDatabase()
        {
            List<string> databaseNames = await SharedProperties.GetDatabases().ConfigureAwait(true);

            var selectDatabaseWindow = new Window
            {
                Title = "Select Database",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            var stackPanel = new StackPanel();
            var comboBox = new ComboBox { Margin = new Thickness(10) };
            foreach (var dbName in databaseNames)
            {
                comboBox.Items.Add(new ComboBoxItem { Content = dbName });
            }
            comboBox.SelectedIndex = 0;

            var okButton = new Button
            {
                Content = "OK",
                Width = 75,
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            okButton.Click += (s, e) => selectDatabaseWindow.DialogResult = true;

            stackPanel.Children.Add(comboBox);
            stackPanel.Children.Add(okButton);
            selectDatabaseWindow.Content = stackPanel;

            if (selectDatabaseWindow.ShowDialog() == true)
            {
                string databaseName = ((ComboBoxItem)comboBox.SelectedItem).Content.ToString();
                if (string.IsNullOrEmpty(databaseName))
                {
                    OutputTextBox.Text = "Database name cannot be empty.";
                    return;
                }

                FeedIterator<ContainerProperties> containerIterator = SharedProperties.CosmosExplorerCore.GetContainerIterator(databaseName);
                OutputTextBox.Text = string.Empty;
                while (containerIterator.HasMoreResults)
                {
                    FeedResponse<ContainerProperties> containerResponse = await containerIterator.ReadNextAsync().ConfigureAwait(true);
                    foreach (var container in containerResponse)
                    {
                        OutputTextBox.Text += $"  Container: {container.Id}" + Environment.NewLine;
                    }
                }
            }
        }

        private async Task RunQueryByDatabaseAndContainer()
        {
            OutputTextBox.Text = string.Empty;

            List<string> databaseNames = await SharedProperties.GetDatabases().ConfigureAwait(true);

            var selectDatabaseWindow = new Window
            {
                Title = "Select Database",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            var stackPanel = new StackPanel();
            var comboBox = new ComboBox { Margin = new Thickness(10) };
            foreach (var dbName in databaseNames)
            {
                comboBox.Items.Add(new ComboBoxItem { Content = dbName });
            }
            comboBox.SelectedIndex = 0;

            var okButton = new Button
            {
                Content = "OK",
                Width = 75,
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            okButton.Click += (s, e) => selectDatabaseWindow.DialogResult = true;

            stackPanel.Children.Add(comboBox);
            stackPanel.Children.Add(okButton);
            selectDatabaseWindow.Content = stackPanel;

            if (selectDatabaseWindow.ShowDialog() == true)
            {
                string databaseName = ((ComboBoxItem)comboBox.SelectedItem).Content.ToString();
                if (string.IsNullOrEmpty(databaseName))
                {
                    OutputTextBox.Text = "Database name cannot be empty.";
                    return;
                }

                var selectContainerWindow = new Window
                {
                    Title = "Select Container",
                    Width = 300,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                var containerStackPanel = new StackPanel();
                var containerComboBox = new ComboBox { Margin = new Thickness(10) };

                FeedIterator<ContainerProperties> containerIterator = SharedProperties.CosmosExplorerCore.GetContainerIterator(databaseName);
                while (containerIterator.HasMoreResults)
                {
                    FeedResponse<ContainerProperties> containerResponse = await containerIterator.ReadNextAsync().ConfigureAwait(true);
                    foreach (var container in containerResponse)
                    {
                        containerComboBox.Items.Add(new ComboBoxItem { Content = container.Id });
                    }
                }
                containerComboBox.SelectedIndex = 0;

                var containerOkButton = new Button
                {
                    Content = "OK",
                    Width = 75,
                    Margin = new Thickness(10),
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                containerOkButton.Click += (s, e) => selectContainerWindow.DialogResult = true;

                containerStackPanel.Children.Add(containerComboBox);
                containerStackPanel.Children.Add(containerOkButton);
                selectContainerWindow.Content = containerStackPanel;

                if (selectContainerWindow.ShowDialog() == true)
                {
                    string containerName = ((ComboBoxItem)containerComboBox.SelectedItem).Content.ToString();
                    if (string.IsNullOrEmpty(containerName))
                    {
                        OutputTextBox.Text = "Container name cannot be empty.";
                        return;
                    }

                    var queryWindow = new Window
                    {
                        Title = "Enter Query",
                        Width = 400,
                        Height = 200,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen,
                        
                    };

                    var queryStackPanel = new StackPanel();
                    var queryTextBox = new TextBox { Margin = new Thickness(10), AcceptsReturn = true, Height = 100, Text = "Select * from c" };

                    var queryOkButton = new Button
                    {
                        Content = "OK",
                        Width = 75,
                        Margin = new Thickness(10),
                        HorizontalAlignment = HorizontalAlignment.Right
                    };
                    queryOkButton.Click += (s, e) => queryWindow.DialogResult = true;

                    queryStackPanel.Children.Add(queryTextBox);
                    queryStackPanel.Children.Add(queryOkButton);
                    queryWindow.Content = queryStackPanel;

                    if (queryWindow.ShowDialog() == true)
                    {
                        string query = queryTextBox.Text;
                        if (string.IsNullOrEmpty(query))
                        {
                            OutputTextBox.Text = "Query cannot be empty.";
                            return;
                        }

                        FeedIterator<dynamic> queryIterator = SharedProperties.CosmosExplorerCore.GetQueryIterator(databaseName, containerName, query);
                        while (queryIterator.HasMoreResults)
                        {
                            FeedResponse<dynamic> queryResponse = await queryIterator.ReadNextAsync().ConfigureAwait(true);
                            foreach (var item in queryResponse)
                            {
                                OutputTextBox.Text += item.ToString() + Environment.NewLine;
                            }
                        }
                    }
                }
            }
        }

        private async Task CreateItemByDatabaseAndContainer()
        {
            OutputTextBox.Text = string.Empty;

            List<string> databaseNames = await SharedProperties.GetDatabases().ConfigureAwait(true);

            var selectDatabaseWindow = new Window
            {
                Title = "Select Database",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            var stackPanel = new StackPanel();
            var comboBox = new ComboBox { Margin = new Thickness(10) };
            foreach (var dbName in databaseNames)
            {
                comboBox.Items.Add(new ComboBoxItem { Content = dbName });
            }
            comboBox.SelectedIndex = 0;

            var okButton = new Button
            {
                Content = "OK",
                Width = 75,
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            okButton.Click += (s, e) => selectDatabaseWindow.DialogResult = true;

            stackPanel.Children.Add(comboBox);
            stackPanel.Children.Add(okButton);
            selectDatabaseWindow.Content = stackPanel;

            if (selectDatabaseWindow.ShowDialog() == true)
            {
                string databaseName = ((ComboBoxItem)comboBox.SelectedItem).Content.ToString();
                if (string.IsNullOrEmpty(databaseName))
                {
                    OutputTextBox.Text = "Database name cannot be empty.";
                    return;
                }

                var selectContainerWindow = new Window
                {
                    Title = "Select Container",
                    Width = 300,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                var containerStackPanel = new StackPanel();
                var containerComboBox = new ComboBox { Margin = new Thickness(10) };

                FeedIterator<ContainerProperties> containerIterator = SharedProperties.CosmosExplorerCore.GetContainerIterator(databaseName);
                while (containerIterator.HasMoreResults)
                {
                    FeedResponse<ContainerProperties> containerResponse = await containerIterator.ReadNextAsync().ConfigureAwait(true);
                    foreach (var container in containerResponse)
                    {
                        containerComboBox.Items.Add(new ComboBoxItem { Content = container.Id });
                    }
                }
                containerComboBox.SelectedIndex = 0;

                var containerOkButton = new Button
                {
                    Content = "OK",
                    Width = 75,
                    Margin = new Thickness(10),
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                containerOkButton.Click += (s, e) => selectContainerWindow.DialogResult = true;

                containerStackPanel.Children.Add(containerComboBox);
                containerStackPanel.Children.Add(containerOkButton);
                selectContainerWindow.Content = containerStackPanel;

                if (selectContainerWindow.ShowDialog() == true)
                {
                    string containerName = ((ComboBoxItem)containerComboBox.SelectedItem).Content.ToString();
                    if (string.IsNullOrEmpty(containerName))
                    {
                        OutputTextBox.Text = "Container name cannot be empty.";
                        return;
                    }

                    var itemWindow = new Window
                    {
                        Title = "Enter Item",
                        Width = 400,
                        Height = 300,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen
                    };

                    var itemStackPanel = new StackPanel();
                    var itemTextBox = new TextBox { Margin = new Thickness(10), AcceptsReturn = true, Height = 150 };
                    var partitionKeyTextBox = new TextBox { Margin = new Thickness(10), Height = 30, Text = "Enter partition key" };

                    var itemOkButton = new Button
                    {
                        Content = "OK",
                        Width = 75,
                        Margin = new Thickness(10),
                        HorizontalAlignment = HorizontalAlignment.Right
                    };
                    itemOkButton.Click += (s, e) => itemWindow.DialogResult = true;

                    itemStackPanel.Children.Add(itemTextBox);
                    itemStackPanel.Children.Add(partitionKeyTextBox);
                    itemStackPanel.Children.Add(itemOkButton);
                    itemWindow.Content = itemStackPanel;

                    if (itemWindow.ShowDialog() == true)
                    {
                        string itemJson = itemTextBox.Text;
                        string partitionKey = partitionKeyTextBox.Text;

                        if (string.IsNullOrEmpty(itemJson) || string.IsNullOrEmpty(partitionKey))
                        {
                            OutputTextBox.Text = "Item and partition key cannot be empty.";
                            return;
                        }

                        dynamic item = Newtonsoft.Json.JsonConvert.DeserializeObject(itemJson);
                        dynamic createdItem = await SharedProperties.CosmosExplorerCore.UpsertItemAsync(databaseName, containerName, item, partitionKey);

                        OutputTextBox.Text = $"Created item: {createdItem}";
                    }
                }
            }
        }

        private async Task DeleteItemByDatabaseAndContainer()
        {
            OutputTextBox.Text = string.Empty;

            List<string> databaseNames = await SharedProperties.GetDatabases().ConfigureAwait(true);

            var selectDatabaseWindow = new Window
            {
                Title = "Select Database",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            var stackPanel = new StackPanel();
            var comboBox = new ComboBox { Margin = new Thickness(10) };
            foreach (var dbName in databaseNames)
            {
                comboBox.Items.Add(new ComboBoxItem { Content = dbName });
            }
            comboBox.SelectedIndex = 0;

            var okButton = new Button
            {
                Content = "OK",
                Width = 75,
                Margin = new Thickness(10),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            okButton.Click += (s, e) => selectDatabaseWindow.DialogResult = true;

            stackPanel.Children.Add(comboBox);
            stackPanel.Children.Add(okButton);
            selectDatabaseWindow.Content = stackPanel;

            if (selectDatabaseWindow.ShowDialog() == true)
            {
                string databaseName = ((ComboBoxItem)comboBox.SelectedItem).Content.ToString();
                if (string.IsNullOrEmpty(databaseName))
                {
                    OutputTextBox.Text = "Database name cannot be empty.";
                    return;
                }

                var selectContainerWindow = new Window
                {
                    Title = "Select Container",
                    Width = 300,
                    Height = 150,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                var containerStackPanel = new StackPanel();
                var containerComboBox = new ComboBox { Margin = new Thickness(10) };

                FeedIterator<ContainerProperties> containerIterator = SharedProperties.CosmosExplorerCore.GetContainerIterator(databaseName);
                while (containerIterator.HasMoreResults)
                {
                    FeedResponse<ContainerProperties> containerResponse = await containerIterator.ReadNextAsync().ConfigureAwait(true);
                    foreach (var container in containerResponse)
                    {
                        containerComboBox.Items.Add(new ComboBoxItem { Content = container.Id });
                    }
                }
                containerComboBox.SelectedIndex = 0;

                var containerOkButton = new Button
                {
                    Content = "OK",
                    Width = 75,
                    Margin = new Thickness(10),
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                containerOkButton.Click += (s, e) => selectContainerWindow.DialogResult = true;

                containerStackPanel.Children.Add(containerComboBox);
                containerStackPanel.Children.Add(containerOkButton);
                selectContainerWindow.Content = containerStackPanel;

                if (selectContainerWindow.ShowDialog() == true)
                {
                    string containerName = ((ComboBoxItem)containerComboBox.SelectedItem).Content.ToString();
                    if (string.IsNullOrEmpty(containerName))
                    {
                        OutputTextBox.Text = "Container name cannot be empty.";
                        return;
                    }

                    var itemWindow = new Window
                    {
                        Title = "Enter Item Details",
                        Width = 400,
                        Height = 200,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen
                    };

                    var itemStackPanel = new StackPanel();
                    var idTextBox = new TextBox { Margin = new Thickness(10), Height = 30, Text = "Enter item id" };
                    var partitionKeyTextBox = new TextBox { Margin = new Thickness(10), Height = 30, Text = "Enter partition key" };

                    var itemOkButton = new Button
                    {
                        Content = "OK",
                        Width = 75,
                        Margin = new Thickness(10),
                        HorizontalAlignment = HorizontalAlignment.Right
                    };
                    itemOkButton.Click += (s, e) => itemWindow.DialogResult = true;

                    itemStackPanel.Children.Add(idTextBox);
                    itemStackPanel.Children.Add(partitionKeyTextBox);
                    itemStackPanel.Children.Add(itemOkButton);
                    itemWindow.Content = itemStackPanel;

                    if (itemWindow.ShowDialog() == true)
                    {
                        string id = idTextBox.Text;
                        string partitionKey = partitionKeyTextBox.Text;

                        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(partitionKey))
                        {
                            OutputTextBox.Text = "Item id and partition key cannot be empty.";
                            return;
                        }

                        dynamic deletedItem = await SharedProperties.CosmosExplorerCore.DeleteItemAsync(databaseName, containerName, id, partitionKey).ConfigureAwait(true);

                        OutputTextBox.Text = $"Deleted item: {deletedItem}";
                    }
                }
            }
        }
    }
}