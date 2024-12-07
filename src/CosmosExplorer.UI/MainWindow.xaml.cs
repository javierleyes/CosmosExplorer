using CosmosExplorer.UI.Common;
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

            SharedProperties.ItemListViewCollection = new ItemListViewCollection();
            ItemListView.ItemsSource = SharedProperties.ItemListViewCollection;
        }

        private async void DatabaseTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is not ContainerTreeSource selectedContainer)
            {
                return;
            }

            SharedProperties.SelectedDatabase = selectedContainer.Database;
            SharedProperties.SelectedContainer = selectedContainer.Name;

            await SharedProperties.LoadItemsAsync().ConfigureAwait(true);
        }

        private async void ItemsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = ItemListView.SelectedItem;
            if (selectedItem is null)
            {
                return;
            }

            string? itemId = selectedItem?.GetType().GetProperty("Id")?.GetValue(selectedItem, null) as string;
            if (string.IsNullOrEmpty(itemId))
            {
                return;
            }

            dynamic item = await SharedProperties.CosmosExplorerCore.GetItemByIdAsync(SharedProperties.SelectedDatabase, SharedProperties.SelectedContainer, itemId).ConfigureAwait(true);

            // Use Dispatcher to update the UI
            Dispatcher.Invoke(() =>
            {
                OutputTextBox.Text = Newtonsoft.Json.JsonConvert.SerializeObject(item, Newtonsoft.Json.Formatting.Indented);
            });
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

        private async void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Validate query.
            await SharedProperties.SearchByQueryAsync(FilterTextBox.Text).ConfigureAwait(true);
        }
    }
}