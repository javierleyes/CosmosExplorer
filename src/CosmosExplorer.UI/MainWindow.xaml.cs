﻿using CosmosExplorer.UI.Common;
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

            SharedProperties.LoaderIndicator = new LoaderIndicator();
            DataContext = SharedProperties.LoaderIndicator;

            SharedProperties.DatabaseCollection = new DatabaseTreeCollection();
            DatabaseTreeView.ItemsSource = SharedProperties.DatabaseCollection;

            SharedProperties.ItemListViewCollection = new ItemListViewCollection();
            ItemListView.ItemsSource = SharedProperties.ItemListViewCollection;
        }

        private async void DatabaseTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            NewItemButton.IsEnabled = false;
            DeleteButton.IsEnabled = false;
            UpdateButton.IsEnabled = false;

            FilterPanel.IsEnabled = false;

            Items.IsEnabled = false;
            ItemListView.IsEnabled = false;
            ItemDescriptionTextBox.IsEnabled = false;

            ItemDescriptionTextBox.Text = string.Empty;

            SharedProperties.ItemListViewCollection.Clear();

            if (e.NewValue is not ContainerTreeSource selectedContainer)
            {
                return;
            }

            await CosmosExplorerHelper.LoadItemsAsync(selectedContainer.Database, selectedContainer.Name).ConfigureAwait(true);

            NewItemButton.IsEnabled = true;

            FilterPanel.IsEnabled = true;

            ItemListView.IsEnabled = true;
            Items.IsEnabled = true;
            ItemDescriptionTextBox.IsEnabled = true;

            ItemDescriptionTextBox.Text = string.Empty;
        }

        private async void ItemsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeleteButton.IsEnabled = false;

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

            string? partitionKey = selectedItem?.GetType().GetProperty("PartitionKey")?.GetValue(selectedItem, null) as string;
            if (string.IsNullOrEmpty(partitionKey))
            {
                return;
            }

            SharedProperties.SelectedItemId = itemId;
            SharedProperties.SelectedItemPartitionKey = partitionKey;

            DeleteButton.IsEnabled = true;

            dynamic item = await CosmosExplorerHelper.GetItemByIdAsync(SharedProperties.SelectedDatabase, SharedProperties.SelectedContainer, SharedProperties.SelectedItemId).ConfigureAwait(true);

            // Use Dispatcher to update the UI
            Dispatcher.Invoke(() =>
            {
                ItemDescriptionTextBox.Text = Newtonsoft.Json.JsonConvert.SerializeObject(item, Newtonsoft.Json.Formatting.Indented);
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
            ItemDescriptionTextBox.Text = string.Empty;

            // TODO: Validate query.
            await CosmosExplorerHelper.SearchByQueryAsync(FilterTextBox.Text).ConfigureAwait(true);
        }

        private void NewItem_Click(object sender, RoutedEventArgs e)
        {
            SaveButton.Visibility = Visibility.Visible;
            SaveButton.IsEnabled = true;

            DiscardButton.Visibility = Visibility.Visible;
            DiscardButton.IsEnabled = true;

            NewItemButton.Visibility = Visibility.Collapsed;
            NewItemButton.IsEnabled = false;

            UpdateButton.Visibility = Visibility.Collapsed;
            UpdateButton.IsEnabled = false;

            DeleteButton.Visibility = Visibility.Collapsed;
            DeleteButton.IsEnabled = false;

            FilterPanel.IsEnabled = false;

            ItemListView.IsEnabled = false;

            ItemDescriptionTextBox.Text = string.Empty;
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Discard_Click(object sender, RoutedEventArgs e)
        {
            SaveButton.Visibility = Visibility.Collapsed;
            SaveButton.IsEnabled = false;

            DiscardButton.Visibility = Visibility.Collapsed;
            DiscardButton.IsEnabled = false;

            NewItemButton.Visibility = Visibility.Visible;
            NewItemButton.IsEnabled = true;

            UpdateButton.Visibility = Visibility.Visible;
            UpdateButton.IsEnabled = false;

            DeleteButton.Visibility = Visibility.Visible;
            DeleteButton.IsEnabled = false;

            FilterPanel.IsEnabled = true;

            ItemListView.IsEnabled = true;
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            await CosmosExplorerHelper.DeleteItemAsync(SharedProperties.SelectedItemId, SharedProperties.SelectedItemPartitionKey).ConfigureAwait(true);

            SharedProperties.ItemListViewCollection.RemoveAt(ItemListView.SelectedIndex);
            ItemDescriptionTextBox.Text = string.Empty;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            string newItem = ItemDescriptionTextBox.Text;
            if (string.IsNullOrWhiteSpace(newItem))
            {
                MessageBox.Show("Item description cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            dynamic newItemJson;
            try
            {
                newItemJson = Newtonsoft.Json.JsonConvert.DeserializeObject(newItem);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Invalid JSON format: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string partitionKey = SharedProperties.ContainerPartitionKey[SharedProperties.SelectedContainer].TrimStart('/');

            string partitionKeyValue = newItemJson[partitionKey].ToString();
            string id = newItemJson["id"].ToString();

            await CosmosExplorerHelper.SaveItemAsync(newItemJson, partitionKeyValue).ConfigureAwait(true);

            SharedProperties.ItemListViewCollection.AddItem(id, partitionKeyValue);

            SaveButton.Visibility = Visibility.Collapsed;
            SaveButton.IsEnabled = false;

            DiscardButton.Visibility = Visibility.Collapsed;
            DiscardButton.IsEnabled = false;

            NewItemButton.Visibility = Visibility.Visible;
            NewItemButton.IsEnabled = true;

            UpdateButton.Visibility = Visibility.Visible;
            UpdateButton.IsEnabled = false;

            DeleteButton.Visibility = Visibility.Visible;
            DeleteButton.IsEnabled = false;

            FilterPanel.IsEnabled = true;

            ItemListView.IsEnabled = true;
        }
    }
}