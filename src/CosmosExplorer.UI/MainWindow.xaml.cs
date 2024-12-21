﻿using CosmosExplorer.UI.Common;
using CosmosExplorer.UI.Modal;
using Newtonsoft.Json.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

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
            SharedProperties.SelectedDatabase = string.Empty;
            SharedProperties.SelectedContainer = string.Empty;
            SharedProperties.SelectedItemId = string.Empty;
            SharedProperties.SelectedItemPartitionKey = string.Empty;
            SharedProperties.SelectedItemJson = null;

            NewItemButton.IsEnabled = false;
            UpdateButton.IsEnabled = false;
            DeleteButton.IsEnabled = false;

            FilterPanel.IsEnabled = false;

            Items.IsEnabled = false;
            ItemListView.IsEnabled = false;
            ItemDescriptionRichTextBox.IsEnabled = false;

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
            ItemDescriptionRichTextBox.IsEnabled = true;

            DisplayJson(string.Empty);
        }

        private async void ItemsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeleteButton.IsEnabled = false;

            DiscardButton.IsEnabled = false;
            DiscardButton.Visibility = Visibility.Collapsed;

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
            SharedProperties.SelectedItemJson = null;

            DeleteButton.IsEnabled = true;

            dynamic item = await CosmosExplorerHelper.GetItemByIdAsync(SharedProperties.SelectedDatabase, SharedProperties.SelectedContainer, SharedProperties.SelectedItemId).ConfigureAwait(true);

            // Use Dispatcher to update the UI
            Dispatcher.Invoke(() =>
            {
                string jsonFromBackend = Newtonsoft.Json.JsonConvert.SerializeObject(item, Newtonsoft.Json.Formatting.Indented);
                DisplayJson(jsonFromBackend);

                TextRange textRange = new TextRange(ItemDescriptionRichTextBox.Document.ContentStart, ItemDescriptionRichTextBox.Document.ContentEnd);
                SharedProperties.SelectedItemJson = textRange.Text;
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

        private void ItemDescriptionRichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextRange textRange = new TextRange(ItemDescriptionRichTextBox.Document.ContentStart, ItemDescriptionRichTextBox.Document.ContentEnd);

            if (SharedProperties.IsCreatingItem)
            {
                DiscardButton.IsEnabled = true;
                DiscardButton.Visibility = Visibility.Visible;

                return;
            }

            if (SharedProperties.SelectedItemJson is null)
            {
                return;
            }

            CosmosExplorerHelper.SetEditMode(SharedProperties.SelectedItemJson, textRange.Text);

            if (SharedProperties.IsEditMode)
            {
                DatabaseTreeView.IsEnabled = false;
                ItemListView.IsEnabled = false;
                FilterPanel.IsEnabled = false;

                NewItemButton.IsEnabled = false;
                NewItemButton.Visibility = Visibility.Collapsed;

                DeleteButton.IsEnabled = false;
                DeleteButton.Visibility = Visibility.Collapsed;

                UpdateButton.IsEnabled = true;

                DiscardButton.IsEnabled = true;
                DiscardButton.Visibility = Visibility.Visible;
            }
            else
            {
                UpdateButton.IsEnabled = false;

                NewItemButton.IsEnabled = true;
                NewItemButton.Visibility = Visibility.Visible;

                DeleteButton.IsEnabled = true;
                DeleteButton.Visibility = Visibility.Visible;

                DiscardButton.IsEnabled = false;
                DiscardButton.Visibility = Visibility.Collapsed;
            }
        }

        private async void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayJson(string.Empty);

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

            SharedProperties.SelectedItemId = string.Empty;
            SharedProperties.SelectedItemPartitionKey = string.Empty;
            SharedProperties.SelectedItemJson = null;

            SharedProperties.IsCreatingItem = true;

            DatabaseTreeView.IsEnabled = false;

            // Important this should be the last line!!!
            DisplayJson(string.Empty);
        }

        private async void Update_Click(object sender, RoutedEventArgs e)
        {
            TextRange textRange = new TextRange(ItemDescriptionRichTextBox.Document.ContentStart, ItemDescriptionRichTextBox.Document.ContentEnd);

            bool result = await CosmosExplorerHelper.UpdateItemAsync(SharedProperties.SelectedItemId, SharedProperties.SelectedItemPartitionKey, textRange.Text).ConfigureAwait(true);

            if (!result)
            {
                return;
            }

            UpdateButton.IsEnabled = false;

            DiscardButton.IsEnabled = false;
            DiscardButton.Visibility = Visibility.Collapsed;

            SharedProperties.IsEditMode = false;
            SharedProperties.SelectedItemJson = null;

            NewItemButton.IsEnabled = true;
            NewItemButton.Visibility = Visibility.Visible;

            DeleteButton.IsEnabled = true;
            DeleteButton.Visibility = Visibility.Visible;

            DatabaseTreeView.IsEnabled = true;
            ItemListView.IsEnabled = true;
            FilterPanel.IsEnabled = true;
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

            FilterPanel.IsEnabled = true;

            DatabaseTreeView.IsEnabled = true;
            ItemListView.IsEnabled = true;

            SharedProperties.SelectedItemJson = null;

            if (SharedProperties.IsEditMode)
            {
                SharedProperties.IsEditMode = false;

                FilterPanel.IsEnabled = true;

                DisplayJson(SharedProperties.SelectedItemJson);
            }

            if (SharedProperties.IsCreatingItem)
            {
                DisplayJson(string.Empty);

                DeleteButton.IsEnabled = false;

                SharedProperties.IsCreatingItem = false;
            }
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            // Show the confirmation modal.
            ConfirmationModal confirmationModal = new ConfirmationModal();
            bool? confirmationResult = confirmationModal.ShowDialog();

            if (confirmationResult is false)
            {
                return;
            }

            bool result = await CosmosExplorerHelper.DeleteItemAsync(SharedProperties.SelectedItemId, SharedProperties.SelectedItemPartitionKey).ConfigureAwait(true);

            if (!result)
            {
                return;
            }

            SharedProperties.ItemListViewCollection.RemoveAt(ItemListView.SelectedIndex);

            DisplayJson(string.Empty);

            UpdateButton.IsEnabled = false;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            TextRange textRange = new TextRange(ItemDescriptionRichTextBox.Document.ContentStart, ItemDescriptionRichTextBox.Document.ContentEnd);

            string newItem = textRange.Text;

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

            string partitionKeyValue = string.Empty;
            try
            {
                partitionKeyValue = newItemJson[partitionKey].ToString();
            }
            catch (Exception)
            {
                MessageBox.Show($"Invalid partition key", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string id = string.Empty;
            try
            {
                id = newItemJson["id"].ToString();
            }
            catch (Exception)
            {
                MessageBox.Show($"Invalid id", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

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

            SharedProperties.IsCreatingItem = false;

            DatabaseTreeView.IsEnabled = true;

            DisplayJson(string.Empty);
        }

        private void DisplayJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                ItemDescriptionRichTextBox.Document.Blocks.Clear();
                return;
            }

            Paragraph paragraph = new Paragraph();
            var jsonObject = JObject.Parse(json);

            AddJsonToken(paragraph, jsonObject, 0);

            ItemDescriptionRichTextBox.Document.Blocks.Clear();
            ItemDescriptionRichTextBox.Document.Blocks.Add(paragraph);
        }

        private void AddJsonToken(Paragraph paragraph, JToken token, int indentLevel, bool isLast = false)
        {
            string indent = new string(' ', indentLevel * 2);

            if (token is JProperty property)
            {
                paragraph.Inlines.Add(new Run($"{indent}\"{property.Name}\": ") { Foreground = Brushes.Maroon });
                AddJsonToken(paragraph, property.Value, indentLevel, isLast);
            }
            else if (token is JObject obj)
            {
                paragraph.Inlines.Add(new Run($"{indent}{{\n"));
                var children = obj.Children().ToList();
                for (int i = 0; i < children.Count; i++)
                {
                    AddJsonToken(paragraph, children[i], indentLevel + 1, i == children.Count - 1);
                }
                paragraph.Inlines.Add(new Run($"{indent}}}{(isLast ? "" : ",")}\n"));
            }
            else if (token is JArray array)
            {
                paragraph.Inlines.Add(new Run($"{indent}[\n"));
                for (int i = 0; i < array.Count(); i++)
                {
                    AddJsonToken(paragraph, array[i], indentLevel + 1, i == array.Count() - 1);
                }
                paragraph.Inlines.Add(new Run($"{indent}]{(isLast ? "" : ",")}\n"));
            }
            else if (token.Type == JTokenType.String)
            {
                if (token.Path == "_etag")
                {
                    paragraph.Inlines.Add(new Run($"{indent}{token.ToString()}{(isLast ? "" : ",")}") { Foreground = Brushes.Navy });
                }
                else
                {
                    paragraph.Inlines.Add(new Run($"{indent}\"{token.ToString()}\"{(isLast ? "" : ",")}") { Foreground = Brushes.Navy });
                }

                paragraph.Inlines.Add(new Run("\n"));
            }
            else if (token.Type == JTokenType.Date)
            {
                DateTime dateTime = token.ToObject<DateTime>();
                string utcDate = dateTime.ToString("o"); // ISO 8601 format
                paragraph.Inlines.Add(new Run($"{indent}\"{utcDate}\"{(isLast ? "" : ",")}") { Foreground = Brushes.Navy });
                paragraph.Inlines.Add(new Run("\n"));
            }
            else
            {
                paragraph.Inlines.Add(new Run($"{indent}{token.ToString()}{(isLast ? "" : ",")}") { Foreground = Brushes.Navy });
                paragraph.Inlines.Add(new Run("\n"));
            }
        }
    }
}