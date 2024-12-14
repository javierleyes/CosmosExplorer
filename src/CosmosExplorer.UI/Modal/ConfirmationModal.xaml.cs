using System.Windows;

namespace CosmosExplorer.UI.Modal
{
    public partial class ConfirmationModal : Window
    {
        public ConfirmationModal()
        {
            InitializeComponent();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
