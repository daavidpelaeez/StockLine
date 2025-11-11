using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.DTOs;

namespace WpfApp1.Views
{
    public partial class MovimientosStockWindow : Window
    {
        private int page = 1;
        private int pageSize = 25;
        private int total = 0;

        private int? _usuarioIdFiltro = null;

        public MovimientosStockWindow(int? usuarioId = null, string usuarioNombre = null)
        {
            InitializeComponent();
            Loaded += MovimientosStockWindow_Loaded;
            _usuarioIdFiltro = usuarioId;
        }

        private async void MovimientosStockWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarProductos();
            await BuscarAsync();
        }

        private async Task CargarProductos()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:5200/");
                    var res = await client.GetAsync("api/productos");
                    if (res.IsSuccessStatusCode)
                    {
                        var json = await res.Content.ReadAsStringAsync();
                        var productos = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ProductoDto>>(json);
                        cbProducto.ItemsSource = productos;
                        cbProducto.SelectedValuePath = "ProductoID";
                        cbProducto.DisplayMemberPath = "Nombre";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando productos: " + ex.Message);
            }
        }

        private async void Buscar_Click(object sender, RoutedEventArgs e)
        {
            page = 1;
            await BuscarAsync();
        }

        private async Task BuscarAsync()
        {
            try
            {
                btnBuscar.IsEnabled = false;
                dgMovimientos.ItemsSource = null;

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:5200/");
                    var query = $"api/movimientosstock?page={page}&pageSize={pageSize}&sortBy=Fecha&sortDir=desc";

                    if (cbProducto.SelectedValue != null)
                        query += $"&productId={cbProducto.SelectedValue}";

                    // Filtro tipo
                    string tipo = null;
                    if (cbTipo.SelectedItem is ComboBoxItem tipoItem)
                        tipo = tipoItem.Content.ToString();
                    if (!string.IsNullOrWhiteSpace(tipo) && tipo != "Todos")
                        query += $"&tipo={tipo}";

                    // Filtro usuario solo por ID
                    if (_usuarioIdFiltro.HasValue)
                        query += $"&usuarioId={_usuarioIdFiltro.Value}";
    
                    if (dpFrom.SelectedDate.HasValue)
                        query += $"&from={dpFrom.SelectedDate.Value:yyyy-MM-dd}";

                    if (dpTo.SelectedDate.HasValue)
                        query += $"&to={dpTo.SelectedDate.Value:yyyy-MM-dd}";

                    var res = await client.GetAsync(query);
                    var body = await res.Content.ReadAsStringAsync();
                    if (!res.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Error fetching movimientos: " + body);
                        return;
                    }

                    // Soporta 'items' o 'Items' y 'total' o 'Total'
                    var token = Newtonsoft.Json.Linq.JToken.Parse(body);
                    var itemsToken = token["items"] ?? token["Items"];
                    var totalToken = token["total"] ?? token["Total"];
                    if (itemsToken == null || itemsToken.Type != Newtonsoft.Json.Linq.JTokenType.Array)
                    {
                        MessageBox.Show("La respuesta del servidor no contiene movimientos o el formato es incorrecto.\n\nRespuesta:\n" + body, "Error de datos", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    total = (int)(totalToken ?? 0);
                    var items = itemsToken.ToObject<List<MovimientoDto>>();

                    dgMovimientos.ItemsSource = items;
                    int totalPages = pageSize > 0 ? (int)Math.Ceiling((double)total / pageSize) : 1;
                    txtPaginacion.Text = $"Página {page} / {totalPages} - {total} registros";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en búsqueda: " + ex.Message);
            }
            finally
            {
                btnBuscar.IsEnabled = true;
            }
        }

        private async void Exportar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                btnExportar.IsEnabled = false;
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:5200/");
                    var query = "api/movimientosstock/export";
                    if (cbProducto.SelectedValue != null) query += $"?productId={cbProducto.SelectedValue}";
                    var res = await client.GetAsync(query);
                    if (!res.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Error exportando CSV: " + await res.Content.ReadAsStringAsync());
                        return;
                    }

                    var bytes = await res.Content.ReadAsByteArrayAsync();
                    var tmp = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "movimientos.csv");
                    System.IO.File.WriteAllBytes(tmp, bytes);
                    MessageBox.Show("CSV exportado a: " + tmp);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exportando: " + ex.Message);
            }
            finally
            {
                btnExportar.IsEnabled = true;
            }
        }

        private void Crear_Click(object sender, RoutedEventArgs e)
        {
            // Pasar el usuario actual si está disponible
            var dlg = _usuarioIdFiltro.HasValue ? new CrearMovimientoWindow(_usuarioIdFiltro.Value) : new CrearMovimientoWindow();
            if (dlg.ShowDialog() == true)
            {
                _ = BuscarAsync();
            }
        }

        private void Anterior_Click(object sender, RoutedEventArgs e)
        {
            if (page > 1)
            {
                page--;
                _ = BuscarAsync();
            }
        }

        private void Siguiente_Click(object sender, RoutedEventArgs e)
        {
            if (page * pageSize < total)
            {
                page++;
                _ = BuscarAsync();
            }
        }

        private void Ver_Click(object sender, RoutedEventArgs e)
        {
            MovimientoDto m = null;
            if (sender is Button btn && btn.DataContext is MovimientoDto row)
                m = row;
            else if (dgMovimientos.SelectedItem is MovimientoDto sel)
                m = sel;
            if (m == null)
            {
                MessageBox.Show("No se pudo obtener el movimiento seleccionado.");
                return;
            }
            try
            {
                var dlg = new VerMovimientoWindow(m.MovimientoID);
                dlg.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error abriendo detalle: " + ex.Message);
            }
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

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BarraSuperior_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}
