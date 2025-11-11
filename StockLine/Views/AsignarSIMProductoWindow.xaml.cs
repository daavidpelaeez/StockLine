using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using WpfApp1.Services;
using WpfApp1.DTOs;

namespace WpfApp1.Views
{
    public partial class AsignarSIMProductoWindow : Window
    {
        private readonly ISIMService _simService;
        private readonly IProductoService _productoService;
        private readonly int _simId;
        private readonly string _numeroSIM;

        public event Action ProductoAsignado;

        public AsignarSIMProductoWindow(int simId, string numeroSIM)
        {
            InitializeComponent();
            
            _simService = new SIMService();
            _productoService = new ProductoService();
            _simId = simId;
            _numeroSIM = numeroSIM;

            txtSubtitulo.Text = "Asignar la SIM '" + _numeroSIM + "' a un producto de la categoria 'Dispositivos con SIM'";

            chkDesasignar.Checked += ChkDesasignar_Checked;
            chkDesasignar.Unchecked += ChkDesasignar_Unchecked;
            btnCancelar.Click += BtnCancelar_Click;
            btnAsignar.Click += BtnAsignar_Click;

            this.Loaded += AsignarSIMProductoWindow_Loaded;
        }

        private async void AsignarSIMProductoWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var todosProductos = await _productoService.GetAllAsync();
                var productosConSIM = todosProductos
                    .Where(p => p.CategoriaNombre != null && 
                               p.CategoriaNombre.Equals("Dispositivos con SIM", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                cbProducto.ItemsSource = productosConSIM;

                if (productosConSIM.Count > 0)
                {
                    cbProducto.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("No hay productos de la categoria 'Dispositivos con SIM' disponibles.", 
                        "Informacion", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar productos: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
            }
        }

        private void ChkDesasignar_Checked(object sender, RoutedEventArgs e)
        {
            cbProducto.IsEnabled = false;
            btnAsignar.Content = "Desasignar";
            btnAsignar.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C"));
        }

        private void ChkDesasignar_Unchecked(object sender, RoutedEventArgs e)
        {
            cbProducto.IsEnabled = true;
            btnAsignar.Content = "Asignar";
            btnAsignar.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F39C12"));
        }

        private async void BtnAsignar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnAsignar.IsEnabled = false;
                bool exito;
                string error;

                if (chkDesasignar.IsChecked == true)
                {
                    (exito, error) = await _simService.DesasignarProductoAsync(_simId);
                    if (exito)
                    {
                        MessageBox.Show("SIM desasignada correctamente", "Exito", MessageBoxButton.OK, MessageBoxImage.Information);
                        ProductoAsignado?.Invoke();
                        this.DialogResult = true;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show($"No se pudo desasignar la SIM: {error}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        btnAsignar.IsEnabled = true;
                    }
                }
                else
                {
                    var productoSeleccionado = cbProducto.SelectedItem as ProductoDto;
                    if (productoSeleccionado == null || productoSeleccionado.ProductoID <= 0)
                    {
                        MessageBox.Show("Selecciona un producto válido", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                        btnAsignar.IsEnabled = true;
                        return;
                    }
                    int productoId = productoSeleccionado.ProductoID;
                    (exito, error) = await _simService.AsignarProductoAsync(_simId, productoId);
                    if (exito)
                    {
                        MessageBox.Show("SIM asignada correctamente al producto", "Exito", MessageBoxButton.OK, MessageBoxImage.Information);
                        ProductoAsignado?.Invoke();
                        this.DialogResult = true;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show($"No se pudo asignar la SIM al producto: {error}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        btnAsignar.IsEnabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                btnAsignar.IsEnabled = true;
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
