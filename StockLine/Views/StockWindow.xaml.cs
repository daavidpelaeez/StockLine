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

        private void TxtBusqueda_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Filtrar productos basado en el texto de búsqueda
            string busqueda = txtBusqueda.Text.ToLower().Trim();
            
            if (string.IsNullOrEmpty(busqueda))
            {
                // Si el campo está vacío, mostrar todos los productos filtrados
                ProductosVM.AplicarBusqueda(string.Empty);
            }
            else
            {
                // Aplicar búsqueda a los productos
                ProductosVM.AplicarBusqueda(busqueda);
            }
        }

        private void BtnLimpiarBusqueda_Click(object sender, RoutedEventArgs e)
        {
            txtBusqueda.Clear();
            ProductosVM.AplicarBusqueda(string.Empty);
            txtBusqueda.Focus();
        }

        private void BtnLimpiarFiltros_Click(object sender, RoutedEventArgs e)
        {
            txtBusqueda.Clear();
            ProductosVM.LimpiarFiltros();
            chkSoloCriticos.IsChecked = false;
            if (ProductosVM.Categorias.Count > 0)
                cbCategoria.SelectedItem = ProductosVM.Categorias[0];
        }
        #endregion

        #region Botones principales
        private void BtnNuevo_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new AddProduct();
            ventana.ShowDialog();
            
            // Recargar después de agregar
            _ = ProductosVM.CargarProductosAsync();
            ActualizarKPIs();
        }

        private void BtnExportar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeWindow_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
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
            txtTotalFooter.Text = ProductosVM.TotalProductos.ToString();
        }

        private void StockGrid_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var row = ItemsControl.ContainerFromElement(StockGrid, e.OriginalSource as DependencyObject) as DataGridRow;
            if (row != null)
                row.IsSelected = true;
        }

        private async void EditarProducto_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var producto = button?.Tag as ProductoDto;
            
            if (producto == null)
            {
                producto = StockGrid.SelectedItem as ProductoDto;
            }
            
            if (producto == null)
            {
                MessageBox.Show("Selecciona un producto primero.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var ventanaEdicion = new AddProduct(producto);
            bool? resultado = ventanaEdicion.ShowDialog();
            if (resultado == true)
            {
                await ProductosVM.CargarProductosAsync();
                ActualizarKPIs();
                ProductoModificado?.Invoke();
            }
        }

        private async void EliminarProducto_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var producto = button?.Tag as ProductoDto;
            
            if (producto == null)
            {
                producto = StockGrid.SelectedItem as ProductoDto;
            }
            
            if (producto == null)
            {
                MessageBox.Show("Selecciona un producto para eliminar.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirmacion = MessageBox.Show(
                $"¿Estás seguro de eliminar el producto '{producto.Nombre}'?\n\nEsta acción NO se puede deshacer.",
                "Confirmar Eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmacion != MessageBoxResult.Yes)
                return;

            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:5200/");
                    
                    System.Diagnostics.Debug.WriteLine($"Eliminando producto ID: {producto.ProductoID}");
                    
                    var response = await client.DeleteAsync($"api/productos/{producto.ProductoID}");
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"Error al eliminar: {errorContent}");
                        
                        MessageBox.Show(
                            $"Error al eliminar el producto:\n\nCódigo: {response.StatusCode}\nDetalle: {errorContent}",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        return;
                    }
                    
                    System.Diagnostics.Debug.WriteLine("Producto eliminado correctamente");
                }
                
                MessageBox.Show("Producto eliminado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                await ProductosVM.CargarProductosAsync();
                ActualizarKPIs();
                ProductoModificado?.Invoke();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Excepción al eliminar: {ex.Message}");
                MessageBox.Show($"Error al eliminar el producto:\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
