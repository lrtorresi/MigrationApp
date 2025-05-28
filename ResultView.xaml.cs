using EnvironmentValidator.Domain.Entities.Workplace;
using EnvironmentValidator.Helpers;
using EnvironmentValidator.Service.Resources;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace EnvironmentValidator
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ResultView : Window
    {
        private readonly object _resultEnvironment;
        private readonly LogManager _logManager;

        public ResultView(object resultEnvironment)
        {
            this.InitializeComponent();

            this.Title = "Ímpar - Validador de Ambiente";
            this.AppWindow.SetIcon("Assets\\StoreLogo.ico");
            this.Maximize();

            _resultEnvironment = resultEnvironment;

            DisplayResult();
        }

        private void DisplayResult()
        {
            var type = _resultEnvironment.GetType();
            txtUserCount.Text = type.GetProperty("TotalUsers")?.GetValue(_resultEnvironment)?.ToString();
            txtCommunityCount.Text = type.GetProperty("TotalGroups")?.GetValue(_resultEnvironment)?.ToString();
            txtPostsCount.Text = type.GetProperty("TotalPosts")?.GetValue(_resultEnvironment)?.ToString();
            txtknowledgeLibraryCount.Text = type.GetProperty("TotalKnowledgeLibrary")?.GetValue(_resultEnvironment)?.ToString();
            txtValueMigration.Text = "Após a análise do ambiente, a aquisição da ferramenta é necessária para realizar a migração.";

            // Exibir dados adicionais em uma tabela dinâmica
            CreateDynamicTable(type.GetProperty("Groups")?.GetValue(_resultEnvironment));
        }

        private void CreateDynamicTable(object data)
        {
            DynamicTableGrid.RowDefinitions.Clear();
            DynamicTableGrid.ColumnDefinitions.Clear();
            DynamicTableGrid.Children.Clear();

            // Definir tres colunas
            DynamicTableGrid.ColumnDefinitions.Add(new ColumnDefinition());
            DynamicTableGrid.ColumnDefinitions.Add(new ColumnDefinition());
            DynamicTableGrid.ColumnDefinitions.Add(new ColumnDefinition());

            // Adicionar a primeira linha para os cabeçalhos
            var headerRow = new RowDefinition();
            DynamicTableGrid.RowDefinitions.Add(headerRow);

            AddTableCell(DynamicTableGrid, "NOME", 0, 0, true);
            AddTableCell(DynamicTableGrid, "ID", 0, 1, true);
            AddTableCell(DynamicTableGrid, "Tamanho Aproximado (MB)", 0, 2, true);

            // Adicionar linhas e células à tabela

            int rowIndex = 1; // Começar após os cabeçalhos
            foreach (var item in (IEnumerable<object>)data)
            {
                var row = new RowDefinition();
                DynamicTableGrid.RowDefinitions.Add(row);

                AddTableCell(DynamicTableGrid, item.GetType().GetProperty("Name")?.GetValue(item)?.ToString(), rowIndex, 0);
                AddTableCell(DynamicTableGrid, item.GetType().GetProperty("Id")?.GetValue(item)?.ToString(), rowIndex, 1);
                AddTableCell(DynamicTableGrid, item.GetType().GetProperty("TotalMb")?.GetValue(item)?.ToString(), rowIndex, 2);

                rowIndex++;
            }
        }

        private void AddTableCell(Grid grid, string text, int row, int column, bool isHeader = false)
        {
            var border = new Border
            {
                BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Black),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(5)
            };

            var textBlock = new TextBlock
            {
                Text = text,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                FontWeight = isHeader ? Microsoft.UI.Text.FontWeights.Bold : Microsoft.UI.Text.FontWeights.Normal
            };

            border.Child = textBlock;
            Grid.SetRow(border, row);
            Grid.SetColumn(border, column);
            grid.Children.Add(border);
        }

        private async void BtnConsultantButton(object sender, RoutedEventArgs e)
        {
            // URL do site para "Falar com um consultor"
            var uri = new Uri("https://impar.com.br/fale-conosco/");
            await Launcher.LaunchUriAsync(uri);
        }

        private async void BtnExportXlsx(object sender, RoutedEventArgs e)
        {
            //Gerar arquivo Excel na pasta Downloads
            var t = new GenerateXlsx();
            var path = await t.ExportToXlsx(_resultEnvironment);
            
            var exportFiles = new GenerateXlsxFIles();
            await exportFiles.ExportToXlsxFiles();
            
            var exportFilesUser = new GenerateXlsxUsers();
            await exportFilesUser.ExportToXlsxUsers();

            ContentDialog dialog = new ContentDialog
            {
                Title = "Exportação de dados",
                Content = $"O arquivo foi salvo em: {path}",
                CloseButtonText = "Fechar"
            };

            dialog.XamlRoot = this.Content.XamlRoot;
            ContentDialogResult result = await dialog.ShowAsync();
        }

    }
}
