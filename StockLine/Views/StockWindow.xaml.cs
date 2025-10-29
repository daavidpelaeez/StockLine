using System;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.DTOs;
using WpfApp1.ViewModels;

namespace WpfApp1.Views
{
    public partial class StockWindow : Window
    {
        private ProductosViewModel vm;

        public StockWindow()
        {
            InitializeComponent();

            vm = new ProductosViewModel();
            this.DataContext = vm;

            this.Loaded += StockWindow_Loaded;

            // Enlazar eventos de filtros
            cbCategoria.SelectionChanged += CbCategoria_SelectionChanged;
            chkSoloCriticos.Checked += ChkSoloCriticos_Changed;
            chkSoloCriticos.Unchecked += ChkSoloCriticos_Changed;
        }

        private async void StockWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Cargar categorías primero para poder filtrar
            await vm.CargarCategoriasAsync();
            await vm.CargarProductosAsync();
            ActualizarKPIs();

            // Enlazar ComboBox al listado de categorías
            cbCategoria.ItemsSource = vm.Categorias;
            cbCategoria.DisplayMemberPath = "Nombre";
            cbCategoria.SelectedValuePath = "CategoriaID";
        }

        #region Filtros
        private void CbCategoria_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbCategoria.SelectedItem != null)
            {
                vm.CategoriaSeleccionada = (CategoriaDto)cbCategoria.SelectedItem;
            }
        }

        private void ChkSoloCriticos_Changed(object sender, RoutedEventArgs e)
        {
            vm.SoloCriticos = chkSoloCriticos.IsChecked == true;
        }

        private void BtnLimpiarFiltros_Click(object sender, RoutedEventArgs e)
        {
            vm.LimpiarFiltros();
            chkSoloCriticos.IsChecked = false;
            if (vm.Categorias.Count > 0)
                cbCategoria.SelectedItem = vm.Categorias[0];
        }
        #endregion

        #region Botones principales
        private void BtnNuevo_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Funcionalidad de nuevo producto pendiente.");
        }

        private void BtnExportar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region Importar / Exportar CSV
        private async void BtnImportar_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv"
            };

            if (openFileDialog.ShowDialog() != true)
                return;

            try
            {
                await vm.ImportarDesdeCsvAsync(openFileDialog.FileName);
                ActualizarKPIs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error importando: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            if (vm.Productos.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.", "Exportar", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                FileName = $"Stock_{DateTime.Now:yyyyMMdd}.csv"
            };

            if (saveFileDialog.ShowDialog() != true) return;

            vm.ExportarAExcel(saveFileDialog.FileName);
        }
        #endregion

        private void ActualizarKPIs()
        {
            txtTotalProductos.Text = vm.TotalProductos.ToString();
            txtCriticos.Text = vm.Criticos.ToString();
            txtUnidades.Text = vm.Unidades.ToString();
        }
    }
}
