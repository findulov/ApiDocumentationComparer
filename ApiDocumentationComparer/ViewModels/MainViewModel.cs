using ApiDocumentationComparer.Commands;
using ApiDocumentationComparer.Infrastructure.Extensions;
using ApiDocumentationComparer.Infrastructure.Handlers;
using ApiDocumentationComparer.Infrastructure.Settings;
using MediatR;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ApiDocumentationComparer.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IMediator mediator;
        private readonly ISettingsManager settingsManager;

        private string firstApiUrl;
        private string secondApiUrl;
        private IEnumerable<SwaggerEndpointConfigurationViewModel> firstApiEndpoints;
        private IEnumerable<SwaggerEndpointConfigurationViewModel> secondApiEndpoints;
        private bool isLoading;

        public MainViewModel(
            IMediator mediator,
            ISettingsManager settingsManager)
        {
            this.mediator = mediator;
            this.settingsManager = settingsManager;

            FirstApiUrl = settingsManager.Get(nameof(FirstApiUrl));
            SecondApiUrl = settingsManager.Get(nameof(SecondApiUrl));

            IsLoading = true;

            FetchEndpointsCommand = new RelayCommand(async obj => await FetchEndpoints());
        }

        public string FirstApiUrl
        {
            get => firstApiUrl;
            set
            {
                firstApiUrl = value;
                OnPropertyChanged(nameof(FirstApiUrl));
                settingsManager.Save(nameof(FirstApiUrl), firstApiUrl);
            }
        }

        public string SecondApiUrl
        {
            get => secondApiUrl;
            set
            {
                secondApiUrl = value;
                OnPropertyChanged(nameof(SecondApiUrl));
                settingsManager.Save(nameof(SecondApiUrl), secondApiUrl);
            }
        }

        public IEnumerable<SwaggerEndpointConfigurationViewModel> FirstApiEndpoints
        {
            get => firstApiEndpoints;
            set
            {
                firstApiEndpoints = value;
                OnPropertyChanged(nameof(FirstApiEndpoints));
            }
        }

        public IEnumerable<SwaggerEndpointConfigurationViewModel> SecondApiEndpoints
        {
            get => secondApiEndpoints;
            set
            {
                secondApiEndpoints = value;
                OnPropertyChanged(nameof(SecondApiEndpoints));
            }
        }

        public bool IsLoading
        {
            get => isLoading;
            set
            {
                isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public ICommand FetchEndpointsCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private async Task FetchEndpoints()
        {
            if (FirstApiUrl.IsNullOrWhiteSpace() || SecondApiUrl.IsNullOrWhiteSpace())
            {
                MessageBox.Show("Please enter both URLs");
                return;
            }

            if (!CheckIfUrlIsValid(FirstApiUrl) || !CheckIfUrlIsValid(SecondApiUrl))
            {
                MessageBox.Show("Please enter valid URLs");
                return;
            }

            IsLoading = true;

            var endpointsResponse = await mediator.Send(new ApiDocumentationRetrieveRequest(FirstApiUrl));
            var endpoints2Response = await mediator.Send(new ApiDocumentationRetrieveRequest(SecondApiUrl));

            IsLoading = false;

            if (endpointsResponse.Errors.Any())
            {
                MessageBox.Show(string.Join(Environment.NewLine, endpointsResponse.Errors));
                return;
            }

            if (endpoints2Response.Errors.Any())
            {
                MessageBox.Show(string.Join(Environment.NewLine, endpoints2Response.Errors));
                return;
            }

            var endpoints = endpointsResponse.Endpoints;
            var endpoints2 = endpoints2Response.Endpoints;

            var newEndpoints = endpoints.Where(e => !endpoints2.Any(e2 => e2.OperationId == e.OperationId)).ToList();
            var deletedEndpoints = endpoints2.Where(e2 => !endpoints.Any(e => e.OperationId == e2.OperationId)).ToList();
            var endpointsWithChangedParameters = endpoints
                .Where(e => endpoints2.Any(e2 =>
                    e2.OperationId == e.OperationId &&
                    (!e.Parameters.Select(p => p.Name).OrderBy(p => p).SequenceEqual(e2.Parameters.Select(p => p.Name).OrderBy(p => p)) ||
                    !e.Properties.Select(p => p.Key).OrderBy(p => p).SequenceEqual(e2.Properties.Select(p => p.Key).OrderBy(p => p)))
                )).ToList();

            FirstApiEndpoints = endpoints.Select(config => new SwaggerEndpointConfigurationViewModel(
                    config: config,
                    isNewEndpoint: newEndpoints.Any(e => e.OperationId == config.OperationId),
                    isDeletedEndpoint: deletedEndpoints.Any(e => e.OperationId == config.OperationId),
                    isUpdatedEndpoint: endpointsWithChangedParameters.Any(e => e.OperationId == config.OperationId)))
                .OrderByDescending(config => config.IsNewEndpoint)
                .ThenByDescending(config => config.IsDeletedEndpoint)
                .ThenByDescending(config => config.IsUpdatedEndpoint)
                .ToList();

            SecondApiEndpoints = endpoints2.Select(config => new SwaggerEndpointConfigurationViewModel(
                    config: config,
                    isNewEndpoint: newEndpoints.Any(e => e.OperationId == config.OperationId),
                    isDeletedEndpoint: deletedEndpoints.Any(e => e.OperationId == config.OperationId),
                    isUpdatedEndpoint: endpointsWithChangedParameters.Any(e => e.OperationId == config.OperationId)))
                .OrderByDescending(config => config.IsDeletedEndpoint)
                .ThenByDescending(config => config.IsNewEndpoint)
                .ThenByDescending(config => config.IsUpdatedEndpoint)
                .ToList();
        }

        private bool CheckIfUrlIsValid(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult) && 
                (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
