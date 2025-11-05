using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using WpfApp1.DTOs;

namespace WpfApp1.Views
{
    public partial class VerMovimientoWindow : Window
    {
        private int id;
        public VerMovimientoWindow(int movimientoId)
        {
            InitializeComponent();
            id = movimientoId;
            Loaded += VerMovimientoWindow_Loaded;
        }

        private async void VerMovimientoWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarDetalle();
        }

        private async Task CargarDetalle()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:5200/");
                    var res = await client.GetAsync($"api/movimientosstock/{id}");
                    var body = await res.Content.ReadAsStringAsync();
                    if (!res.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Error obteniendo detalle: " + body);
                        return;
                    }

                    var mov = Newtonsoft.Json.JsonConvert.DeserializeObject<MovimientoDto>(body);
                    txtFecha.Text = $"Fecha: {mov.Fecha:dd/MM/yyyy HH:mm}";
                    txtTipo.Text = $"Tipo: {mov.TipoMovimiento}";
                    txtProducto.Text = $"Producto: {mov.ProductoNombre}";
                    txtCantidad.Text = $"Cantidad: {mov.Cantidad}";
                    txtUsuario.Text = $"Usuario: {mov.UsuarioNombre}";
                    txtObservaciones.Text = $"Observaciones: {mov.Observaciones}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando detalle: " + ex.Message);
            }
        }

        private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
