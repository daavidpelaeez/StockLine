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

        public MovimientosStockWindow()
        {
            InitializeComponent();
            Loaded += MovimientosStockWindow_Loaded;
        }

        private async void MovimientosStockWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarProductosAsync();
            await BuscarAsync();
        }

        private async Task CargarProductosAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:5200/");
                    var res = await client.GetAsync("api/productos");
                    var body = await res.Content.ReadAsStringAsync();
                    if (!res.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Error obteniendo productos: " + body);
                        return;
                    }
                    var productos = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ProductoDto>>(body);
                    cbProducto.ItemsSource = productos;
                    cbProducto.DisplayMemberPath = "Nombre";
                    cbProducto.SelectedValuePath = "ProductoID";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando productos: " + ex.Message);
            }
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

                   
                    string tipo = null;
                    if (cbTipo.SelectedItem is ComboBoxItem tipoItem)
                        tipo = tipoItem.Content.ToString();
                    if (!string.IsNullOrWhiteSpace(tipo) && tipo != "Todos")
                        query += $"&tipo={tipo}";

                   
                    if (dpFrom.SelectedDate.HasValue)
                        query += $"&from={dpFrom.SelectedDate.Value:yyyy-MM-dd}";
                    if (dpTo.SelectedDate.HasValue)
                        query += $"&to={dpTo.SelectedDate.Value:yyyy-MM-dd}";

                    var res = await client.GetAsync(query);
                    var body = await res.Content.ReadAsStringAsync();
                    if (!res.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Error obteniendo movimientos: " + body);
                        return;
                    }

                    var token = Newtonsoft.Json.Linq.JToken.Parse(body);
                    List<MovimientoDto> items = null;
                    if (token.Type == Newtonsoft.Json.Linq.JTokenType.Array)
                    {
                        items = token.ToObject<List<MovimientoDto>>();
                        total = items.Count;
                    }
                    else
                    {
                        var itemsToken = token["items"] ?? token["Items"];
                        var totalToken = token["total"] ?? token["Total"];
                        if (itemsToken == null || itemsToken.Type != Newtonsoft.Json.Linq.JTokenType.Array)
                        {
                            MessageBox.Show("La respuesta del servidor no contiene movimientos o el formato es incorrecto.\n\nRespuesta:\n" + body, "Error de datos", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        total = (int)(totalToken ?? 0);
                        items = itemsToken.ToObject<List<MovimientoDto>>();
                    }
                   
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

        
        private async void Ver_Click(object sender, RoutedEventArgs e)
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
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:5200/");
                    var res = await client.GetAsync($"api/movimientosstock/{m.MovimientoID}");
                    var body = await res.Content.ReadAsStringAsync();
                    if (!res.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Error obteniendo detalles: " + body);
                        return;
                    }
                    var detalle = Newtonsoft.Json.JsonConvert.DeserializeObject<MovimientoDto>(body);
                   
                    MessageBox.Show($"Movimiento: {detalle.MovimientoID}\nTipo: {detalle.Tipo}\nFecha: {detalle.Fecha}\nUsuario: {detalle.UsuarioNombre}\nProductos: {string.Join(", ", detalle.Productos)}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error abriendo detalle: " + ex.Message);
            }
        }

        private async void Buscar_Click(object sender, RoutedEventArgs e)
        {
            page = 1;
            await BuscarAsync();
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
