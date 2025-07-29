using EnvironmentValidator.Domain.Entities.Global;
using EnvironmentValidator.Domain.Entities.Workplace;
using EnvironmentValidator.Helpers;
using EnvironmentValidator.Service.Workplace;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using Windows.UI.Popups;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EnvironmentValidator
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly EnvironmentValidatorServices _environmentValidator;
        public MainViewModel ViewModel { get; } = new MainViewModel();

        public MainWindow(EnvironmentValidatorServices environmentValidator)
        {
            this.InitializeComponent();
            this.Maximize();
            this.AppWindow.SetIcon("Assets\\StoreLogo.ico");
            _environmentValidator = environmentValidator;

            // “Content” é o root do seu XAML (no seu caso o <Grid> que você definiu).
            if (this.Content is FrameworkElement root)
            {
                root.DataContext = ViewModel;
            }
        }

        private async void getTokenWorkplace(object sender, RoutedEventArgs e)
        {
            var textBoxValueToken = inputTokenWorkplace.Text.Trim();

            var validTokenWorkplace = await ValidToken(textBoxValueToken);
            if (!validTokenWorkplace) { return; }

            // Esconda os elementos iniciais
            ViewInitialPanel.Visibility = Visibility.Collapsed;

            // Exiba o indicador de loading
            LoadingPanel.Visibility = Visibility.Visible;

            var progress = new Progress<string>(msg => ViewModel.StatusMessage = msg);
            var result = await _environmentValidator.EnvironmentValidator(textBoxValueToken, progress);

            if (result == null)
            {
                //Informa que o token é inválido
                await ValidToken("");
                return;
            }

            // Exiba o resultado
            var resultView = new ResultView(result);
            resultView.Activate();

            //Feche a janela atual
            LoadingPanel.Visibility = Visibility.Collapsed;
            this.Close();
        }

        private async Task<bool> ValidToken(string textBoxValue)
        {
            if (textBoxValue == "" | textBoxValue.Length < 20)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Alerta!",
                    Content = "Verifique se o campo Token foi digitado corretamente.",
                    CloseButtonText = "Ok"
                };

                ViewInitialPanel.Visibility = Visibility.Visible;
                LoadingPanel.Visibility = Visibility.Collapsed;

                dialog.XamlRoot = this.Content.XamlRoot;
                ContentDialogResult result = await dialog.ShowAsync();
                return false;
            }

            return true;
        }

        
    }
}
