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
using MigrationApp.Services.Migration;
using MigrationApp.Services.Resources;
using MigrationApp.Services.Workplace;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

namespace MigrationApp
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CommunitiesViews : Window
    {
        private readonly LogManager _logManager;
        private readonly GroupWorkplaceService _groupWorkplaceService;
        private readonly MigrationService _migrationService;
        private List<string> selectedIds = new List<string>();
        private List<Option> options = new List<Option>();

        public MainViewModel ViewModel { get; } = new MainViewModel();

        public CommunitiesViews(LogManager logManager, GroupWorkplaceService groupWorkplaceService, MigrationService migrationService)
        {
            this.InitializeComponent();
            this.Maximize();
            this.AppWindow.SetIcon("Assets\\StoreLogo.ico");
            _logManager = logManager;
            _groupWorkplaceService = groupWorkplaceService;
            _migrationService = migrationService;
                        
            if (this.Content is FrameworkElement root)
            {
                root.DataContext = ViewModel;
            }

            LoadOptionsAsync();
        }

        private async Task LoadOptionsAsync()
        {
            //Ligar o loading
            LoadingPanel.Visibility = Visibility.Visible;

            //Get Groups Workplace
            var groups = await _groupWorkplaceService.GetAll();

            if (groups != null)
            {
                //Desligar o loading
                LoadingPanel.Visibility = Visibility.Collapsed;

                foreach (var item in groups.Data)
                {
                    var checkBox = new CustomCheckBox
                    {
                        Content = item.Name,
                        Tag = item.Id,
                        Privacy = item.Privacy,
                        Cover = item.Cover?.Source,
                        Margin = new Thickness(24, 0, 0, 0)
                    };
                    checkBox.Checked += Option_Checked;
                    checkBox.Unchecked += Option_Unchecked;
                    OptionsPanel.Children.Add(checkBox);
                }
            }
        }

        private async void Option_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CustomCheckBox;
            if (checkBox != null && checkBox.Tag is string id)
            {
                selectedIds.Add(id);

                options.Add(new Option { Id = id, Name = checkBox.Content.ToString(), Privacy = checkBox.Privacy.ToString(), CouverSource = checkBox?.Cover?.ToString() ?? "" });
                UpdateSelectedItemsList(sender);
            }
        }

        private void Option_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (checkBox != null && checkBox.Tag is string id)
            {
                selectedIds.Remove(id);

                //remove de options o item selecionado
                options.RemoveAll(o => o.Id == id);
                UpdateSelectedItemsList(sender);
            }
        }

        private void SelectAll_Checked(object sender, RoutedEventArgs e)
        {
            foreach (var child in OptionsPanel.Children)
            {
                if (child is CheckBox checkBox && checkBox != OptionsAllCheckBox)
                {
                    checkBox.IsChecked = true;
                }
            }
        }

        private void SelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (var child in OptionsPanel.Children)
            {
                if (child is CheckBox checkBox && checkBox != OptionsAllCheckBox)
                {
                    checkBox.IsChecked = false;
                }
            }

            //limpar options
            options.Clear();
            selectedIds.Clear();
            SelectedItemsListBox.Items.Clear();
        }

        private void SelectAll_Indeterminate(object sender, RoutedEventArgs e)
        {
            // Implementação, se necessário
        }

        private void UpdateSelectedItemsList(object sender)
        {       
            SelectedItemsListBox.Items.Clear();
            foreach (var id in selectedIds)
            {
                var option = options.Find(o => o.Id == id);
                if (option != null)
                {
                    SelectedItemsListBox.Items.Add(option.Name);
                }
            }
            int selectedCount = SelectedItemsListBox.Items.Count;
            SelectedCountTextBlock.Text = $"Total de comunidades selecionadas: {selectedCount}/{GlobalConfig.TotalCommunityCout}";
        }

        private async void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            //Verifica quantas comunidades podem ser migradas
            //if (selectedIds.Count > GlobalConfig.TotalCommunityCout)
            //{   
            //    await ShowCommunityLimitAlert();
            //    return;
            //}

            var dialog = new ContentDialog
            {
                Title = "Migração de comunidades",
                Content = "Você confirma que deseja migrar as comunidades selecionadas?",
                PrimaryButtonText = "Sim",
                SecondaryButtonText = "Não"
            };

            dialog.XamlRoot = this.Content.XamlRoot;
            ContentDialogResult result = await dialog.ShowAsync();

            var iconOk = new FontIcon
            {
                Glyph = "&#xE73E;",
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.Green),
                FontSize = 16,
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            var iconError = new FontIcon
            {
                Glyph = "&#xE8BB;",
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red),
                FontSize = 16,
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            if (result == ContentDialogResult.Primary)
            {
                ContinueButton.IsEnabled = false;
                checkBoxGrid.Visibility = Visibility.Collapsed;
                LoadingMigrationPanel.Visibility = Visibility.Visible;

                SelectedItemsListBox.Items.Clear(); //Limpar a lista de seleção
                var progress = new Progress<string>(msg => ViewModel.StatusMessage = msg);

                //Migrando as comunidades
                foreach (var option in options)
                {
                    var createCommunity = await _migrationService.MigrateGroups(option.Name, option.Id, option.Privacy, option.CouverSource, progress);

                    if (createCommunity)
                    {
                        SelectedItemsListBox.Items.Add($"{option.Name} - ✅");
                    }
                    else
                    {
                        SelectedItemsListBox.Items.Add($"{option.Name} - ❌");
                    }
                }

                handleMigrationCompleted();
            }

            else { return; }
        }

        private async void handleMigrationCompleted()
        {
            var dialog = new ContentDialog
            {
                Title = "Migração de comunidades concluída com sucesso ✅.",
                PrimaryButtonText = "Continuar"
            };

            dialog.XamlRoot = this.Content.XamlRoot;
            ContentDialogResult result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                //Abre a tela de migração de Biblioteca de Conhecimento
                //Se tudo estiver correto vai para a proxima tela
                var knowledgeLibraryView = App.Services.GetRequiredService<KnowledgeLibraryView>();
                knowledgeLibraryView.Activate();

                //Feche a janela atual
                this.Close();
            }
        }

        private async Task ShowCommunityLimitAlert()
        {
            var dialog = new ContentDialog
            {
                Title = $"Atenção, voce só pode selecionar {GlobalConfig.TotalCommunityCout} comunidades.",
                PrimaryButtonText = "OK"
            };

            dialog.XamlRoot = this.Content.XamlRoot;
            ContentDialogResult result = await dialog.ShowAsync();

            
        }

        public class Option
        {
            public string? Id { get; set; }
            public string? Name { get; set; }
            public string? Privacy { get; set; }
            public string? CouverSource { get; set; }
        }

        public class CustomCheckBox : CheckBox
        {
            public string? Cover { get; set; }
            public string? Privacy { get; set; }
        }
    }
}
