using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using WpfApp1.Services;

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
                bool resultado;

                if (chkDesasignar.IsChecked == true)
                {
                    resultado = await _simService.DesasignarProductoAsync(_simId);

                    if (resultado)
                    {
                        MessageBox.Show("SIM desasignada correctamente", "Exito", MessageBoxButton.OK, MessageBoxImage.Information);
                        if (ProductoAsignado != null)
                            ProductoAsignado();
                        this.DialogResult = true;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("No se pudo desasignar la SIM", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        btnAsignar.IsEnabled = true;
                    }
                }
                else
                {
                    if (cbProducto.SelectedValue == null)
                    {
                        MessageBox.Show("Selecciona un producto", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                        btnAsignar.IsEnabled = true;
                        return;
                    }

                    int productoId = (int)cbProducto.SelectedValue;
                    resultado = await _simService.AsignarProductoAsync(_simId, productoId);

                    if (resultado)
                    {
                        MessageBox.Show("SIM asignada correctamente al producto", "Exito", MessageBoxButton.OK, MessageBoxImage.Information);
                        if (ProductoAsignado != null)
                            ProductoAsignado();
                        this.DialogResult = true;
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("No se pudo asignar la SIM al producto", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
