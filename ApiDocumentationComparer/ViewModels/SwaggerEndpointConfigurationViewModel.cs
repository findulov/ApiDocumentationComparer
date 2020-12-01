using ApiDocumentationComparer.Infrastructure.Models;
using System.ComponentModel;

namespace ApiDocumentationComparer.ViewModels
{
    public class SwaggerEndpointConfigurationViewModel : INotifyPropertyChanged
    {
        public SwaggerEndpointConfigurationViewModel(
            SwaggerEndpointConfiguration config,
            bool isNewEndpoint,
            bool isDeletedEndpoint,
            bool isUpdatedEndpoint)
        {
            Config = config;
            IsNewEndpoint = isNewEndpoint;
            IsDeletedEndpoint = isDeletedEndpoint;
            IsUpdatedEndpoint = isUpdatedEndpoint;
        }

        private bool isNewEndpoint;
        private bool isDeletedEndpoint;
        private bool isUpdatedEndpoint;

        public SwaggerEndpointConfiguration Config { get; set; }

        public bool IsNewEndpoint
        { 
            get => isNewEndpoint;
            set
            {
                isNewEndpoint = value;
                OnPropertyChanged(nameof(IsNewEndpoint));
            }
        }

        public bool IsDeletedEndpoint
        {
            get => isDeletedEndpoint;
            set
            {
                isDeletedEndpoint = value;
                OnPropertyChanged(nameof(IsDeletedEndpoint));
            }
        }

        public bool IsUpdatedEndpoint
        {
            get => isUpdatedEndpoint;
            set
            {
                isUpdatedEndpoint = value;
                OnPropertyChanged(nameof(IsUpdatedEndpoint));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
