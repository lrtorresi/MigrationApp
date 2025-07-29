using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MigrationApp.Entities.Global;
using MigrationApp.Helpers;
using MigrationApp.Services.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MigrationApp
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly ValidatorService _validator;
        private readonly LogManager _logManager;

        public MainWindow(ValidatorService validator, LogManager logManager)
        {
            this.InitializeComponent();
            this.Maximize();
            this.AppWindow.SetIcon("Assets\\StoreLogo.ico");
            _validator = validator;
            _logManager = logManager;
        }

        private async void validTokenImpar(object sender, RoutedEventArgs e)
        {
            _logManager.LogInformation("-- Migrador Iniciado");
            

            // Esconda os elementos iniciais
            ViewInitialPanel.Visibility = Visibility.Collapsed;

            // Exiba o indicador de loading
            LoadingPanel.Visibility = Visibility.Visible;

            //Valida se o token ímpar é valido           
            var validKey = await _validator.KeyValidation(inputTokenImpar.Text.Trim());

            if (validKey == 0)
            {
                await ErrorView();
                return;
            }

            // Avança para a proxima tela
            var configurationsView = App.Services.GetRequiredService<ConfigurationsView>();
            configurationsView.Activate();

            //Feche a janela atual
            LoadingPanel.Visibility = Visibility.Collapsed;
            this.Close();
        }

        private async Task ErrorView()
        {
            _logManager.LogError("Chave Ímpar incorreta.");
            ContentDialog dialog = new ContentDialog
            {
                Title = "Alerta!",
                Content = "Verifique se a chave foi digitada corretamente.",
                CloseButtonText = "Ok"
            };

            ViewInitialPanel.Visibility = Visibility.Visible;
            LoadingPanel.Visibility = Visibility.Collapsed;

            dialog.XamlRoot = this.Content.XamlRoot;
            ContentDialogResult result = await dialog.ShowAsync();
        }
    }
}
