using System.Windows;

namespace CosmosExplorer.UI
{
    /// <summary>
    /// Interaction logic for ManageConnections.xaml
    /// </summary>
    public partial class ManageConnectionsModal : Window
    {
        public ManageConnectionsModal()
        {
            InitializeComponent();
            DataContext = new ManageConnectionsViewModel();
            ConnectionsDataGrid.ItemsSource = ((ManageConnectionsViewModel)DataContext).Connections;
        }
    }
}
