using System.Windows;

namespace CosmosExplorer.UI
{
    /// <summary>
    /// Interaction logic for SaveConnectionStringModal.xaml
    /// </summary>
    public partial class SaveConnectionStringModal : Window
    {
        public SaveConnectionStringModal()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ConnectionStringNameTextBox.Text))
            {
                MessageBox.Show("The connection string can not be empty", "Save connection string", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true;

            this.Close();
        }
    }
}
