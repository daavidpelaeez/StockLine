using System;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.DTOs;
using WpfApp1.ViewModels;
using WpfApp1.Views;

namespace WpfApp1.Views
{
    public partial class StockWindow : Window
    {
        private ProductosViewModel ProductosVM;

        public event Action ProductoModificado;

        public StockWindow()
        {
            InitializeComponent();

            ProductosVM = new ProductosViewModel();
            this.DataContext = ProductosVM;

            this.Loaded += StockWindow_Loaded;


            cbCategoria.SelectionChanged += CbCategoria_SelectionChanged;
            chkSoloCriticos.Checked += ChkSoloCriticos_Changed;
            chkSoloCriticos.Unchecked += ChkSoloCriticos_Changed;
        }

        private async void StockWindow_Loaded(object sender, RoutedEventArgs e)
        {

            await ProductosVM.CargarCategoriasAsync();
            await ProductosVM.CargarProductosAsync();
            ActualizarKPIs();

            cbCategoria.ItemsSource = ProductosVM.Categorias;
            cbCategoria.DisplayMemberPath = "Nombre";
            cbCategoria.SelectedValuePath = "CategoriaID";
        }

        #region Filtros
        private void CbCategoria_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbCategoria.SelectedItem != null)
            {
                ProductosVM.CategoriaSeleccionada = (CategoriaDto)cbCategoria.SelectedItem;
            }
        }

        private void ChkSoloCriticos_Changed(object sender, RoutedEventArgs e)
        {
            ProductosVM.SoloCriticos = chkSoloCriticos.IsChecked == true;
        }

        private void BtnLimpiarFiltros_Click(object sender, RoutedEventArgs e)
        {
            ProductosVM.LimpiarFiltros();
            chkSoloCriticos.IsChecked = false;
            if (ProductosVM.Categorias.Count > 0)
                cbCategoria.SelectedItem = ProductosVM.Categorias[0];
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
                await ProductosVM.ImportarDesdeCsvAsync(openFileDialog.FileName);
                ActualizarKPIs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error importando: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            if (ProductosVM.Productos.Count == 0)
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

            ProductosVM.ExportarAExcel(saveFileDialog.FileName);
        }
        #endregion

        private void ActualizarKPIs()
        {
            txtTotalProductos.Text = ProductosVM.TotalProductos.ToString();
            txtCriticos.Text = ProductosVM.Criticos.ToString();
            txtUnidades.Text = ProductosVM.Unidades.ToString();
        }

        private void StockGrid_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var row = ItemsControl.ContainerFromElement(StockGrid, e.OriginalSource as DependencyObject) as DataGridRow;
            if (row != null)
                row.IsSelected = true;
        }

        private async void EditarProducto_Click(object sender, RoutedEventArgs e)
        {
            var producto = StockGrid.SelectedItem as ProductoDto;
            if (producto == null)
            {
                MessageBox.Show("Selecciona un producto primero.");
                return;
            }

            var ventanaEdicion = new AddProduct(producto);
            bool? resultado = ventanaEdicion.ShowDialog();
            if (resultado == true)
            {
                ProductoModificado?.Invoke();
            }
        }

        private void EliminarProducto_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
