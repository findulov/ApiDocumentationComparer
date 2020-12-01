using ApiDocumentationComparer;
using ApiDocumentationComparer.ApplicationSettings;
using ApiDocumentationComparer.Infrastructure.Settings;
using ApiDocumentationComparer.ViewModels;
using MediatR;
using MediatR.SimpleInjector;
using SimpleInjector;
using System;
using System.Reflection;

internal static class Program
{
    [STAThread]
    internal static void Main()
    {
        var container = Bootstrap();
        RunApplication(container);
    }

    private static Container Bootstrap()
    {
        // Create the container as usual.
        var container = new Container();

        Assembly infrastructureAssembly = Assembly.Load("ApiDocumentationComparer.Infrastructure");

        container.BuildMediator(infrastructureAssembly);
        container.Register(typeof(IRequestHandler<>), infrastructureAssembly);

        container.Register<MainWindow>();
        container.Register<MainViewModel>();

        container.Register<ISettingsManager, ApplicationSettingsManager>();

        container.Verify();

        return container;
    }

    private static void RunApplication(Container container)
    {
        try
        {
            var app = new App();
            var mainWindow = container.GetInstance<MainWindow>();
            app.Run(mainWindow);
        }
        catch (Exception ex)
        {
            //Log the exception and exit
        }
    }
}