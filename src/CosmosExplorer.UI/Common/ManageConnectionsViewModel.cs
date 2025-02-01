using CosmosExplorer.Core;
using CosmosExplorer.UI;
using CosmosExplorer.UI.Common;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

public class ManageConnectionsViewModel
{
    public ObservableCollection<Connection> Connections { get; set; }

    public ManageConnectionsViewModel()
    {
        ObservableCollection<Connection> connections = new ObservableCollection<Connection>();
        foreach (string connectionName in SharedProperties.SavedConnections.Keys)
        {
            var connection = new Connection { Name = connectionName };
            connection.DeleteCommand = new RelayCommand(param => DeleteConnection(connection));
            connections.Add(connection);
        }

        Connections = connections;
    }

    private void DeleteConnection(Connection connection)
    {
        // Check if the key already exists
        if (!SharedProperties.SavedConnections.ContainsKey(connection.Name))
        {
            MessageBox.Show("This connection string does not exist.", "Manage connections", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        this.Connections.Remove(connection);

        SharedProperties.SavedConnections.Remove(connection.Name);

        string userSettingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CosmosExplorer", SharedProperties.UserSettingsFileName);

        string savedConnections = JsonSerializer.Serialize(SharedProperties.SavedConnections, new JsonSerializerOptions { WriteIndented = true });

        string encryptedSavedConnections = Utils.Encrypt(savedConnections, SharedProperties.Key, SharedProperties.IV);

        File.WriteAllText(userSettingsPath, encryptedSavedConnections);

        if (Application.Current.MainWindow is MainWindow mainWindowInstance)
        {
            if (SharedProperties.SavedConnections.Count == 0)
            {
                mainWindowInstance.ManageConnectionsMenuItem.IsEnabled = false;
                SharedProperties.SavedConnections.Clear();
                mainWindowInstance.SavedConnectionMenuItem.IsEnabled = false;
            }
            else
            {
                var itemToRemove = mainWindowInstance.SavedConnectionMenuItem.Items
                            .OfType<MenuItem>()
                            .FirstOrDefault(item => item.Header.ToString() == connection.Name);

                if (itemToRemove != null)
                {
                    mainWindowInstance.SavedConnectionMenuItem.Items.Remove(itemToRemove);
                }
            }
        }
    }
}

public class Connection
{
    public string Name { get; set; }
    public ICommand DeleteCommand { get; set; }
}