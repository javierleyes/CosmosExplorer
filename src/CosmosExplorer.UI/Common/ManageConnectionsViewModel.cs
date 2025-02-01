using CosmosExplorer.UI.Common;
using System.Collections.ObjectModel;
using System.Windows.Input;

public class ManageConnectionsViewModel
{
    public ObservableCollection<Connection> Connections { get; set; }

    public ManageConnectionsViewModel()
    {
        Connections = new ObservableCollection<Connection>
        {
            new Connection { Name = "Connection1", EditCommand = new RelayCommand(EditConnection), DeleteCommand = new RelayCommand(DeleteConnection) },
            new Connection { Name = "Connection2", EditCommand = new RelayCommand(EditConnection), DeleteCommand = new RelayCommand(DeleteConnection) }
        };
    }

    private void EditConnection(object parameter)
    {
        // Implement edit logic here
    }

    private void DeleteConnection(object parameter)
    {
        // Implement delete logic here
    }
}

public class Connection
{
    public string Name { get; set; }
    public ICommand EditCommand { get; set; }
    public ICommand DeleteCommand { get; set; }
}