using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using MigrationApp.Services.Engage;
using MigrationApp.Services.Migration;
using MigrationApp.Services.Resources;
using MigrationApp.Services.Workplace;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MigrationApp
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    /// 
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        /// 

        public static IConfiguration Configuration { get; private set; }
        public static IServiceProvider Services { get; private set; }


        public App()
        {
            this.InitializeComponent();

            InitializeConfiguration();

            Services = ConfigureServices();
        }

        private void InitializeConfiguration()
        {

            //Cria a pasta onde o Log será salvo
            string path = @"C:\log";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
           
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
                .AddScoped<GroupWorkplaceService>()
                .AddScoped<UsersWorkplaceService>()
                .AddScoped<PostWorkplaceService>()
                .AddScoped<knowledgeLibraryService>()
                .AddScoped<ValidatorService>()
                .AddScoped<MigrationService>()
                .AddScoped<GenerateHtml>()
                .AddScoped<GeneratePdf>()
                .AddScoped<UserEngageService>()
                .AddScoped<CommunityEngageService>()
                .AddScoped<PostEngageService>()                
                .AddSingleton<IConfiguration>(builder)
                .AddLogging(configure => configure.AddSerilog())
                .AddSingleton<LogManager>()                
                .AddTransient<MainWindow>()
                .AddTransient<ConfigurationsView>()
                .AddTransient<CommunitiesViews>()
                .AddTransient<KnowledgeLibraryView>()
                .BuildServiceProvider();

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
            m_window = Services.GetRequiredService<ConfigurationsView>();
            //m_window = new MainWindow();
            m_window.Activate();
        }

        private Window m_window;
    }
}
