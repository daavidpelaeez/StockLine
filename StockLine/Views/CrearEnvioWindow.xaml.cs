using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.DTOs;
using WpfApp1.Services;
using WpfApp1;

namespace WpfApp1.Views
{
    public partial class CrearEnvioWindow : Window
    {
        private readonly IEnvioService _envioService;
        private readonly IAyuntamientoService _ayuntamientoService;
        private readonly IComercialService _comercialService;
        private readonly IProductoService _productoService;
        private readonly ISIMService _simService;
        private readonly int _usuarioId;

        private ObservableCollection<ProductoEnvioTemp> _productosEnvio;
        private List<ProductoDto> _productosDisponibles;

        // ? Constante para la categoría que puede llevar SIM
        private const string CATEGORIA_CON_SIM = "Dispositivos con SIM";

        public event Action EnvioCreado;

        public CrearEnvioWindow(int usuarioId)
        {
            InitializeComponent();
            _envioService = new EnvioService();
            _ayuntamientoService = new AyuntamientoService();
            _comercialService = new ComercialService();
            _productoService = new ProductoService();
            _simService = new SIMService();
            _usuarioId = usuarioId;

            _productosEnvio = new ObservableCollection<ProductoEnvioTemp>();
            dgProductos.ItemsSource = _productosEnvio;

            dpFechaEnvio.SelectedDate = DateTime.Now;

            this.Loaded += CrearEnvioWindow_Loaded;

            // Autoselección y bloqueo de comercial si el usuario es Comercial
            if (Session.RoleID == 2 && Session.ComercialID.HasValue)
            {
                this.Loaded += (s, e) =>
                {
                    // Buscar el comercial en el ComboBox y seleccionarlo
                    foreach (var item in cbComercial.Items)
                    {
                        var prop = item.GetType().GetProperty("ComercialID");
                        if (prop != null && (int)prop.GetValue(item) == Session.ComercialID.Value)
                        {
                            cbComercial.SelectedItem = item;
                            cbComercial.IsEnabled = false;
                            break;
                        }
                    }
                };
            }
        }

        private async void CrearEnvioWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarDatos();
        }

        private async Task CargarDatos()
        {
            try
            {
                var ayuntamientos = await _ayuntamientoService.GetAllAsync();
                cbAyuntamiento.ItemsSource = ayuntamientos;
                if (ayuntamientos.Count > 0)
                    cbAyuntamiento.SelectedIndex = 0;

                var comerciales = await _comercialService.GetAllAsync();
                cbComercial.ItemsSource = comerciales;

                // Selección automática del comercial logueado
                if (Session.RoleID == 2 && Session.ComercialID.HasValue)
                {
                    var comercial = comerciales.FirstOrDefault(c => c.ComercialID == Session.ComercialID.Value);
                    if (comercial != null)
                    {
                        cbComercial.SelectedItem = comercial;
                        cbComercial.IsEnabled = false;
                    }
                }
                else if (comerciales.Count > 0)
                {
                    cbComercial.SelectedIndex = 0;
                }

                _productosDisponibles = await _productoService.GetAllAsync();
                cbProducto.ItemsSource = _productosDisponibles;
                if (_productosDisponibles.Count > 0)
                    cbProducto.SelectedIndex = 0;

                // Ya no cargamos todas las SIMs aquí
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RecargarSIMsDisponiblesParaProducto(int productoId)
        {
            try
            {
                var todasLasSIMs = await _simService.GetAllAsync();
                var simsDisponibles = todasLasSIMs
                    .Where(s => s.ProductoID == productoId && string.IsNullOrEmpty(s.Ubicacion))
                    .ToList();
                simsDisponibles.Insert(0, new SIMDTO
                {
                    SIMID = 0,
                    NumeroSIM = "Sin SIM",
                    ProductoID = productoId
                });
                cbSIM.ItemsSource = simsDisponibles;
                cbSIM.SelectedIndex = 0;
                cbSIM.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al recargar SIMs: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                cbSIM.IsEnabled = false;
                cbSIM.ItemsSource = null;
            }
        }

        private async Task RecargarSIMsAsociadasAProducto(int productoId)
        {
            try
            {
                var todasLasSIMs = await _simService.GetAllAsync();
                var simsAsociadas = todasLasSIMs
                    .Where(s => s.ProductoID == productoId)
                    .ToList();
                simsAsociadas.Insert(0, new SIMDTO
                {
                    SIMID = 0,
                    NumeroSIM = "Sin SIM",
                    ProductoID = productoId
                });
                cbSIM.ItemsSource = simsAsociadas;
                cbSIM.SelectedIndex = 0;
                cbSIM.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar SIMs: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                cbSIM.IsEnabled = false;
                cbSIM.ItemsSource = null;
            }
        }

        private async Task RecargarSIMsEnAlmacenParaProducto(int productoId)
        {
            try
            {
                var todasLasSIMs = await _simService.GetAllAsync();
                var simsEnAlmacen = todasLasSIMs
                    .Where(s => s.Ubicacion != null && s.Ubicacion.Trim().Equals("En almacén", StringComparison.OrdinalIgnoreCase)
                        && s.ProductoID == productoId)
                    .ToList();
                simsEnAlmacen.Insert(0, new SIMDTO
                {
                    SIMID = 0,
                    NumeroSIM = "Sin SIM",
                    ProductoID = productoId
                });
                cbSIM.ItemsSource = simsEnAlmacen;
                cbSIM.SelectedIndex = 0;
                cbSIM.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar SIMs: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                cbSIM.IsEnabled = false;
                cbSIM.ItemsSource = null;
            }
        }

        private async void cbProducto_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var productoSeleccionado = cbProducto.SelectedItem as ProductoDto;
            if (productoSeleccionado == null)
            {
                cbSIM.IsEnabled = false;
                cbSIM.ItemsSource = null;
                return;
            }

            bool puedeLlevarSIM = productoSeleccionado.CategoriaNombre != null && 
                                  productoSeleccionado.CategoriaNombre.Equals(CATEGORIA_CON_SIM, StringComparison.OrdinalIgnoreCase);

            if (puedeLlevarSIM)
            {
                await RecargarSIMsEnAlmacenParaProducto(productoSeleccionado.ProductoID);
            }
            else
            {
                cbSIM.IsEnabled = false;
                cbSIM.ItemsSource = null;
            }
        }

        private void btnAgregarProducto_Click(object sender, RoutedEventArgs e)
        {
            var productoSeleccionado = cbProducto.SelectedItem as ProductoDto;
            
            if (productoSeleccionado == null)
            {
                MessageBox.Show("Selecciona un producto", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtCantidad.Text, out int cantidad) || cantidad <= 0)
            {
                MessageBox.Show("Ingresa una cantidad valida", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cantidad > productoSeleccionado.Stock)
            {
                var resultado = MessageBox.Show(
                    $"La cantidad solicitada ({cantidad}) es mayor al stock disponible ({productoSeleccionado.Stock}).\nDeseas continuar de todas formas?",
                    "Stock Insuficiente",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (resultado != MessageBoxResult.Yes)
                    return;
            }

            int? simId = null;
            string simNumero = null;
            
            var simSeleccionada = cbSIM.SelectedItem as SIMDTO;
            if (cbSIM.IsEnabled && simSeleccionada != null)
            {
                if (simSeleccionada.SIMID > 0)
                {
                    simId = simSeleccionada.SIMID;
                    simNumero = simSeleccionada.NumeroSIM;
                    // Solo preguntar si la ubicación NO es 'En almacén'
                    if (!string.IsNullOrEmpty(simSeleccionada.Ubicacion) && !simSeleccionada.Ubicacion.Trim().Equals("En almacén", StringComparison.OrdinalIgnoreCase))
                    {
                        var confirmacion = MessageBox.Show($"La SIM '{simNumero}' ya está asignada a la ubicación '{simSeleccionada.Ubicacion}'. ¿Deseas modificar la ubicación?", "Confirmar modificación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (confirmacion != MessageBoxResult.Yes)
                            return;
                    }
                }
            }

            var productoExistente = _productosEnvio.FirstOrDefault(p => 
                p.ProductoID == productoSeleccionado.ProductoID && 
                p.SIMID == simId);
            
            if (productoExistente != null)
            {
                productoExistente.Cantidad += cantidad;
            }
            else
            {
                _productosEnvio.Add(new ProductoEnvioTemp
                {
                    ProductoID = productoSeleccionado.ProductoID,
                    ProductoNombre = productoSeleccionado.Nombre,
                    Cantidad = cantidad,
                    SIMID = simId,
                    SIMNumero = simNumero ?? "-"
                });
            }

            txtCantidad.Text = "1";
            if (cbSIM.IsEnabled)
                cbSIM.SelectedIndex = 0;
            
            txtValidacion.Visibility = Visibility.Collapsed;
        }

        private void btnEliminarProducto_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var producto = button?.DataContext as ProductoEnvioTemp;
            
            if (producto != null)
            {
                _productosEnvio.Remove(producto);
            }
        }

        private async void btnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarFormulario())
                return;

            // Validar stock actual antes de guardar
            if (!await ValidarStockActualAsync())
                return;

            try
            {
                btnGuardar.IsEnabled = false;

                var nuevoEnvio = new CrearEnvioDTO
                {
                    AyuntamientoID = (int)cbAyuntamiento.SelectedValue,
                    ComercialID = (int)cbComercial.SelectedValue,
                    FechaEnvio = dpFechaEnvio.SelectedDate ?? DateTime.Now,
                    NumeroReferencia = string.IsNullOrWhiteSpace(txtNumeroReferencia.Text) ? null : txtNumeroReferencia.Text,
                    UsuarioID = _usuarioId,
                    Productos = _productosEnvio.Select(p => new CrearEnvioDetalleDTO
                    {
                        ProductoID = p.ProductoID,
                        Cantidad = p.Cantidad,
                        SIMID = p.SIMID
                    }).ToList()
                };

                var resultado = await _envioService.CreateAsync(nuevoEnvio);

                if (resultado != null)
                {
                    MessageBox.Show("Envio creado correctamente", "Exito", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    if (EnvioCreado != null)
                        EnvioCreado();
                    
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("No se pudo crear el envio", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    btnGuardar.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al crear el envio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                btnGuardar.IsEnabled = true;
            }
        }

        private bool ValidarFormulario()
        {
            if (cbAyuntamiento.SelectedValue == null)
            {
                MessageBox.Show("Selecciona un ayuntamiento", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (cbComercial.SelectedValue == null)
            {
                MessageBox.Show("Selecciona un comercial", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!dpFechaEnvio.SelectedDate.HasValue)
            {
                MessageBox.Show("Selecciona una fecha de envio", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (_productosEnvio.Count == 0)
            {
                txtValidacion.Visibility = Visibility.Visible;
                MessageBox.Show("Debes agregar al menos un producto", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void btnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private async void btnActualizarSIMs_Click(object sender, RoutedEventArgs e)
        {
            var productoSeleccionado = cbProducto.SelectedItem as ProductoDto;
            if (productoSeleccionado != null)
            {
                await RecargarSIMsDisponiblesParaProducto(productoSeleccionado.ProductoID);
            }
        }

        private async Task<bool> ValidarStockActualAsync()
        {
            foreach (var producto in _productosEnvio)
            {
                var productoActual = await _productoService.GetByIdAsync(producto.ProductoID);
                if (productoActual == null)
                {
                    MessageBox.Show($"No se pudo obtener el stock actual del producto '{producto.ProductoNombre}'.", "Error de stock", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                if (producto.Cantidad > productoActual.Stock)
                {
                    MessageBox.Show($"No hay suficiente stock para el producto '{producto.ProductoNombre}'.\nSolicitado: {producto.Cantidad}, Disponible: {productoActual.Stock}.", "Stock insuficiente", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }
            return true;
        }
    }

    public class ProductoEnvioTemp
    {
        public int ProductoID { get; set; }
        public string ProductoNombre { get; set; }
        public int Cantidad { get; set; }
        public int? SIMID { get; set; }
        public string SIMNumero { get; set; }
    }
}
