using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Serilog;
using System.Security.Authentication.ExtendedProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Windows.UI.ViewManagement;
using EnvironmentValidator.Service.Workplace;
using EnvironmentValidator.Service.Resources;
using EnvironmentValidator.Domain.Entities.Global;
using EnvironmentValidator.Helpers;



// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EnvironmentValidator
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        /// 
        public static IConfiguration Configuration { get; private set; }
        public IServiceProvider Services { get; }

        private Window _splashWindow;
        private Window _mainWindow;

        public App()
        {
            this.InitializeComponent();
           
            InitializeConfiguration();

            Services = ConfigureServices();
        }

        private void InitializeConfiguration()
        {
            // Configuração do Serilog
            Log.Logger = new LoggerConfiguration()
                 .WriteTo.File("C:\\log\\Log - .txt", rollingInterval: RollingInterval.Day)
                 .CreateLogger();
        }

        private static IServiceProvider ConfigureServices()
        {
            var builder = new ConfigurationBuilder()               
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).Build();


            var services = new ServiceCollection()
                .AddScoped<EnvironmentValidatorServices>()
                .AddScoped<GroupWorkplaceService>()
                .AddScoped<knowledgeLibraryService>()
                .AddScoped<PostWorkplaceService>()
                .AddScoped<UsersWorkplaceService>()
                .AddSingleton<IConfiguration>(builder)
                .AddLogging(configure => configure.AddSerilog())
                .AddSingleton<LogManager>()
                .AddTransient<MainWindow>()
                .BuildServiceProvider();

            ServiceLocator.ServiceProvider = services;
            return services;
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var mainWindow = Services.GetRequiredService<MainWindow>();
            // Configurar a janela para iniciar em tela cheia
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.FullScreen;

            mainWindow.Title = "Ímpar - Validador de Ambiente";
            
            mainWindow.Activate();
        }

        private Window m_window;
    }
}
