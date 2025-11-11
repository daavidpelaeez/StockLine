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
                producto.Activo == false ?
                $"¿Quieres activar el producto '{producto.Nombre}'?" :
                $"¿Estás seguro de desactivar el producto '{producto.Nombre}'?\n\nEsta acción NO se puede deshacer.",
                producto.Activo == false ? "Confirmar Activación" : "Confirmar Desactivación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (confirmacion != MessageBoxResult.Yes)
                return;
            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:5200/");
                    if (producto.Activo == false)
                    {
                        // Activar (PUT)
                        var dto = new { ProductoID = producto.ProductoID, Activo = true };
                        var json = Newtonsoft.Json.JsonConvert.SerializeObject(dto);
                        var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
                        var res = await client.PutAsync($"api/productos/{producto.ProductoID}", content);
                        if (!res.IsSuccessStatusCode)
                        {
                            MessageBox.Show($"Error activando: {await res.Content.ReadAsStringAsync()}");
                            return;
                        }
                    }
                    else
                    {
                        // Desactivar (DELETE)
                        var res = await client.DeleteAsync($"api/productos/{producto.ProductoID}");
                        if (!res.IsSuccessStatusCode)
                        {
                            MessageBox.Show($"Error desactivando: {await res.Content.ReadAsStringAsync()}");
                            return;
                        }
                    }
                }
                MessageBox.Show(producto.Activo == false ? "Producto activado correctamente." : "Producto desactivado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                await ProductosVM.CargarProductosAsync();
                ActualizarKPIs();
                ProductoModificado?.Invoke();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cambiar estado del producto:\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
