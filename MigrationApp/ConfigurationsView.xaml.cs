using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using MigrationApp.Helpers;
using MigrationApp.Services.Workplace;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.Extensions.Configuration;
using MigrationApp.Entities.Global;
using MigrationApp.Services.Resources;
using MigrationApp.Services.Engage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;


namespace MigrationApp
{
    public sealed partial class ConfigurationsView : Window
    {
        private readonly GroupWorkplaceService _groupWorkplaceService;
        private readonly IConfiguration _configuration;
        private readonly LogManager _logManager;
        private readonly UserEngageService _userEngageService;

        public ConfigurationsView(GroupWorkplaceService groupWorkplaceService, IConfiguration configuration, LogManager logManager, UserEngageService userEngageService)
        {
            this.InitializeComponent();
            this.Maximize();
            this.AppWindow.SetIcon("Assets\\StoreLogo.ico");
            _groupWorkplaceService = groupWorkplaceService;
            _configuration = configuration;
            _logManager = logManager;
            _userEngageService = userEngageService;
        }

        private async void SubmitAuthButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            _logManager.LogInformation("-- Configuração de parametros iniciada.");

            //Atualiza os componentes na tela
            await initConfigsLayout();

            //Atualiza os valores Globais
            await updateGlobalConfigs();

            //Valida cada um dos inputs
            var validInputs = await validViewInputs();

            if (!validInputs)
                return;

            //Se tudo estiver correto vai para a proxima tela
            var communitiesViews = App.Services.GetRequiredService<CommunitiesViews>();

            //var communitiesViews = App.Services.GetRequiredService<CommunitiesViews>();
            communitiesViews.Activate();

            //Feche a janela atual
            LoadingPanel.Visibility = Visibility.Collapsed;
            this.Close();
        }

        private async Task initConfigsLayout()
        {
            SubmitButton.IsEnabled = false; //Desabilita o botão
            NewInputForm.Visibility = Visibility.Collapsed; //Esconde os elementos iniciais
            LoadingPanel.Visibility = Visibility.Visible; // Exibe o indicador de loading
            BackButtonPanel.Visibility = Visibility.Visible; //Exibe o botão voltar

            //Input workplace
            inputTokenWorkplace.BorderBrush = new SolidColorBrush(Colors.Gray);
            inputTokenWorkplace.BorderThickness = new Thickness(1);

            //Input Viva Engage
            inputTokenEngage.BorderBrush = new SolidColorBrush(Colors.Gray);
            inputTokenEngage.BorderThickness = new Thickness(1);
            inputClientId.BorderBrush = new SolidColorBrush(Colors.Gray);
            inputClientId.BorderThickness = new Thickness(1);

            //Input Azure
            inputTenantIdAzure.BorderBrush = new SolidColorBrush(Colors.Gray);
            inputTenantIdAzure.BorderThickness = new Thickness(1);
            inputClientIdAzure.BorderBrush = new SolidColorBrush(Colors.Gray);
            inputClientIdAzure.BorderThickness = new Thickness(1);

            //Input Login
            //inputUsername.BorderBrush = new SolidColorBrush(Colors.Gray);
            //inputUsername.BorderThickness = new Thickness(1);
            //inputPassword.BorderBrush = new SolidColorBrush(Colors.Gray);
            //inputPassword.BorderThickness = new Thickness(1);
        }

        private async Task updateGlobalConfigs()
        {
            GlobalConfig.TokenWorkplace = inputTokenWorkplace.Text.Trim();
            GlobalConfig.TokenEngage = inputTokenEngage.Text.Trim();
            GlobalConfig.ClientIdEngage = inputClientId.Text.Trim();
            GlobalConfig.TenatId = inputTenantIdAzure.Text.Trim();
            GlobalConfig.ClientId = inputClientIdAzure.Text.Trim();
            //GlobalConfig.Username = inputUsername.Text.Trim();
            //GlobalConfig.Password = inputPassword.Password.Trim();
            //GlobalConfig.Domain = inputUsername.Text.Trim().Split('@')[1] ?? null;
        }

        private async Task<bool> validViewInputs()
        {
            bool hasError = true;

            //Valida token do Meta Workplace
            var groups = await _groupWorkplaceService.GetAll();
            if (groups == null)
            {
                // Borda vermelha para indicar erro
                inputTokenWorkplace.BorderBrush = new SolidColorBrush(Colors.Red);
                inputTokenWorkplace.BorderThickness = new Thickness(3);

                hasError = false;
            }

            //Valida token do Engage
            var getCurretUser = await _userEngageService.GetUserTokenEngage(["https://api.yammer.com/user_impersonation"]);
            if (getCurretUser == null)
            {
                //Input Viva Engage
                inputTokenEngage.BorderBrush = new SolidColorBrush(Colors.Red);
                inputTokenEngage.BorderThickness = new Thickness(3);
                //inputClientId.BorderBrush = new SolidColorBrush(Colors.Red);
                //inputClientId.BorderThickness = new Thickness(3);

                hasError = false;
            }

            //Valida dados do AppRegistration do Azure e Login Microsoft

            //Limpa cache do token
            await _userEngageService.AuthLimparCacheAsync();
            var getToken = await _userEngageService.GetUserTokenEngage(["https://api.yammer.com/user_impersonation"]);
            if (getToken == null)
            {
                //Input Azure
                inputTenantIdAzure.BorderBrush = new SolidColorBrush(Colors.Red);
                inputTenantIdAzure.BorderThickness = new Thickness(3);
                inputClientIdAzure.BorderBrush = new SolidColorBrush(Colors.Red);
                inputClientIdAzure.BorderThickness = new Thickness(3);

                //Input Login
                //inputUsername.BorderBrush = new SolidColorBrush(Colors.Red);
                //inputUsername.BorderThickness = new Thickness(3);
                //inputPassword.BorderBrush = new SolidColorBrush(Colors.Red);
                //inputPassword.BorderThickness = new Thickness(3);

                hasError = false;
            }

            if (!hasError) { await ErrorView(); }

            return hasError;
        }

        private async void BackButton_Click(object sender, RoutedEventArgs e)
        {
            await ErrorView();
        }

        private async Task ErrorView()
        {
            _logManager.LogError("Chave (configs) incorreta.");
            ContentDialog dialog = new ContentDialog
            {
                Title = "Alerta!",
                Content = "Verifique se as configurações estão corretas e tente novamente.",
                CloseButtonText = "Ok"
            };

            NewInputForm.Visibility = Visibility.Visible;
            LoadingPanel.Visibility = Visibility.Collapsed;
            BackButtonPanel.Visibility = Visibility.Collapsed;
            SubmitButton.IsEnabled = true; //Habilita o botão

            dialog.XamlRoot = this.Content.XamlRoot;
            ContentDialogResult result = await dialog.ShowAsync();
        }
    }
}
