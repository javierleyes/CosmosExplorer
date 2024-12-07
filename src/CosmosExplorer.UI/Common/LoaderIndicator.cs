using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CosmosExplorer.UI
{
    public class LoaderIndicator : INotifyPropertyChanged
    {
        private bool _isLoading;

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void SetLoaderIndicator(bool isLoading)
        {
            IsLoading = isLoading;
        }
    }
}
