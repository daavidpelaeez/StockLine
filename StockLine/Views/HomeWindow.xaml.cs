using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WpfApp1.ViewModels;
using WpfApp1.Views;
using WpfApp1.Services;

namespace WpfApp1.Views
{
    public partial class HomeWindow : Window
    {
        public string NombreUsuario { get; set; }
        public int UsuarioID { get; set; }
        public int RoleID { get; set; }
        public ProductosViewModel ProductosVM { get; set; } = new ProductosViewModel();

        public int MaximoProgressBar { get; set; } = 2000;

        private int MaxProductosMostrar = 5;
        private DispatcherTimer timerNotificaciones;
        private const int STOCK_MINIMO = 10; // Umbral para considerar stock bajo
        private NotificacionService _notificacionService;

        public event Action ProductoModificado;
        
        public HomeWindow(String usuario, int usuarioId = 1, int roleId = 1)
        {
            InitializeComponent();
            NombreUsuario = usuario;
            UsuarioID = usuarioId;
            RoleID = roleId;
            DataContext = this;
            
            // Inicializar servicio de notificaciones
            _notificacionService = new NotificacionService();
            
            // Cargar datos iniciales
            CargarProductosHome();
            
            // Iniciar timer para actualizar notificaciones automáticamente cada 30 segundos
            InicializarTimerNotificaciones();
        }

        #region NOTIFICACIONES

        private bool panelNotificacionesAbierto = false;

        private void ToggleNotificaciones_Click(object sender, MouseButtonEventArgs e)
        {
            panelNotificacionesAbierto = !panelNotificacionesAbierto;
            
            if (panelNotificacionesAbierto)
            {
                panelNotificaciones.Visibility = Visibility.Visible;
                flechaNotificaciones.Text = "▲";
            }
            else
            {
                panelNotificaciones.Visibility = Visibility.Collapsed;
                flechaNotificaciones.Text = "▼";
            }
        }

        private void InicializarTimerNotificaciones()
        {
            timerNotificaciones = new DispatcherTimer();
            timerNotificaciones.Interval = TimeSpan.FromSeconds(30); // Actualizar cada 30 segundos
            timerNotificaciones.Tick += async (s, e) => await ActualizarNotificaciones();
            timerNotificaciones.Start();
            
            // Cargar notificaciones iniciales
            _ = ActualizarNotificaciones();
        }

        private async Task ActualizarNotificaciones()
        {
            try
            {
                int totalNotificaciones = 0;

                // 1. Verificar productos con stock bajo
                int productosStockBajo = await ObtenerProductosStockBajo();
                
                // Verificar si esta notificación fue descartada
                bool stockBajoDescartado = _notificacionService.FueDescartada("StockBajo");
                
                if (productosStockBajo > 0 && !stockBajoDescartado)
                {
                    notifStockBajo.Visibility = Visibility.Visible;
                    txtStockBajo.Text = productosStockBajo == 1 
                        ? "1 producto con stock bajo" 
                        : $"{productosStockBajo} productos con stock bajo";
                    totalNotificaciones += productosStockBajo;
                }
                else
                {
                    notifStockBajo.Visibility = Visibility.Collapsed;
                }

                // 2. Verificar envíos pendientes (solo estado "Pendiente")
                int enviosPendientes = await ObtenerEnviosPendientes();
                
                // Verificar si esta notificación fue descartada
                bool enviosPendientesDescartados = _notificacionService.FueDescartada("EnviosPendientes");
                
                if (enviosPendientes > 0 && !enviosPendientesDescartados)
                {
                    notifEnviosPendientes.Visibility = Visibility.Visible;
                    txtEnviosPendientes.Text = enviosPendientes == 1 
                        ? "1 envio pendiente" 
                        : $"{enviosPendientes} envios pendientes";
                    totalNotificaciones += 1; // Contamos como 1 notificación aunque haya varios envíos
                }
                else
                {
                    notifEnviosPendientes.Visibility = Visibility.Collapsed;
                }

                // 3. Actualizar badge de notificaciones
                if (totalNotificaciones > 0)
                {
                    badgeNotificaciones.Visibility = Visibility.Visible;
                    txtNumNotificaciones.Text = totalNotificaciones.ToString();
                    sinNotificaciones.Visibility = Visibility.Collapsed;
                }
                else
                {
                    badgeNotificaciones.Visibility = Visibility.Collapsed;
                    sinNotificaciones.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al actualizar notificaciones: " + ex.Message);
            }
        }

        private async Task<int> ObtenerProductosStockBajo()
        {
            try
            {
                // Asegurarse de que los productos están cargados
                if (ProductosVM.ProductosFiltrados == null || !ProductosVM.ProductosFiltrados.Any())
                {
                    await ProductosVM.CargarProductosAsync();
                }

                // Contar productos con stock por debajo del mínimo
                var productosStockBajo = ProductosVM.ProductosFiltrados
                    .Where(p => p.Stock < STOCK_MINIMO)
                    .Count();

                return productosStockBajo;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al obtener productos con stock bajo: " + ex.Message);
                return 0;
            }
        }

        private async Task<int> ObtenerEnviosPendientes()
        {
            try
            {
                // Llamada real a la API para obtener envíos
                var envioService = new EnvioService();
                var todosLosEnvios = await envioService.GetAllAsync();
                
                if (todosLosEnvios == null)
                {
                    return 0;
                }

                // Contar SOLO los envíos que están en estado "Pendiente"
                var enviosPendientes = todosLosEnvios
                    .Where(e => e.Estado != null && e.Estado.Equals("Pendiente", StringComparison.OrdinalIgnoreCase))
                    .Count();

                System.Diagnostics.Debug.WriteLine($"Total envíos: {todosLosEnvios.Count}, Pendientes: {enviosPendientes}");
                
                return enviosPendientes;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al obtener envíos pendientes: " + ex.Message);
                return 0;
            }
        }

        private async void ActualizarNotificaciones_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            try
            {
                button.IsEnabled = false;
                button.Content = "🔄 Actualizando...";

                // Limpiar notificaciones expiradas (48 horas)
                _notificacionService.LimpiarNotificacionesExpiradas(48);

                await ActualizarNotificaciones();
                await Task.Delay(500); // Pequeño delay para feedback visual

                button.Content = "✅ Actualizado";
                await Task.Delay(1000);
                button.Content = "🔄 Actualizar";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al actualizar notificaciones: " + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                button.Content = "🔄 Actualizar";
            }
            finally
            {
                button.IsEnabled = true;
            }
        }

        private async void NotificacionStockBajo_Click(object sender, MouseButtonEventArgs e)
        {
            // Abrir ventana de stock filtrada por productos con stock bajo
            StockWindow stock = new StockWindow();

            stock.ProductoModificado += async () =>
            {
                // Limpiar notificación de stock bajo cuando se abre la ventana de stock
                _notificacionService.LimpiarTodasLasNotificaciones();
                
                await ProductosVM.CargarProductosAsync();
                MostrarProductosEnStock();
                await ActualizarNotificaciones(); // Actualizar notificaciones después de modificar
            };

            stock.ShowDialog();
        }

        private void NotificacionEnviosPendientes_Click(object sender, MouseButtonEventArgs e)
        {
            // Abrir ventana de envíos pendientes
            bool esAdmin = RoleID == 3;
            EnviosPendientesWindow enviosWindow = new EnviosPendientesWindow(UsuarioID, esAdmin);
            enviosWindow.ShowDialog();
            
            // Actualizar notificaciones al cerrar la ventana
            _ = ActualizarNotificaciones();
        }

        private void DescartarStockBajo_Click(object sender, RoutedEventArgs e)
        {
            // Marcar la notificación como descartada
            _notificacionService.DescarrarNotificacion("StockBajo");
            
            // Ocultar la notificación de stock bajo
            notifStockBajo.Visibility = Visibility.Collapsed;
            
            // Recalcular el número de notificaciones visibles
            ActualizarBadgeNotificaciones();
        }

        private void DescartarEnviosPendientes_Click(object sender, RoutedEventArgs e)
        {
            // Marcar la notificación como descartada
            _notificacionService.DescarrarNotificacion("EnviosPendientes");
            
            // Ocultar la notificación de envíos pendientes
            notifEnviosPendientes.Visibility = Visibility.Collapsed;
            
            // Recalcular el número de notificaciones visibles
            ActualizarBadgeNotificaciones();
        }

        private void ActualizarBadgeNotificaciones()
        {
            // Contar notificaciones visibles
            int totalNotificaciones = 0;
            
            if (notifStockBajo.Visibility == Visibility.Visible)
                totalNotificaciones++;
            
            if (notifEnviosPendientes.Visibility == Visibility.Visible)
                totalNotificaciones++;
            
            // Actualizar badge
            if (totalNotificaciones > 0)
            {
                badgeNotificaciones.Visibility = Visibility.Visible;
                txtNumNotificaciones.Text = totalNotificaciones.ToString();
                sinNotificaciones.Visibility = Visibility.Collapsed;
            }
            else
            {
                badgeNotificaciones.Visibility = Visibility.Collapsed;
                sinNotificaciones.Visibility = Visibility.Visible;
            }
        }

        #endregion

        private async void CargarProductosHome()
        {
            await ProductosVM.CargarProductosAsync();
            MostrarProductosEnStock();
        }

        private void MostrarProductosEnStock()
        {
            panelProductos.HorizontalAlignment = HorizontalAlignment.Stretch;
            panelProductos.Children.Clear();

            var productosFiltrados = ProductosVM.ProductosFiltrados
                                            .Where(p => p.Stock <= MaximoProgressBar)
                                            .Take(MaxProductosMostrar);

            foreach (var p in productosFiltrados)
            {
                // Border principal
                var border = new Border
                {
                    Background = Brushes.White,
                    CornerRadius = new CornerRadius(12),
                    Padding = new Thickness(0, 10, 0, 10),
                    Margin = new Thickness(0, 0, 0, 16),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    MinWidth = 0,
                    Width = double.NaN // Permite que se estire
                };

                // Grid principal para el contenido del producto
                var grid = new Grid();
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                // StackPanel horizontal para nombre y badge
                var headerStack = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(16, 0, 0, 0)
                };

                var txtNombre = new TextBlock
                {
                    Text = p.Nombre,
                    FontWeight = FontWeights.Bold,
                    FontSize = 15,
                    Foreground = (Brush)new BrushConverter().ConvertFromString("#2C3E50"),
                    VerticalAlignment = VerticalAlignment.Center
                };

                var badgeBorder = new Border
                {
                    Background = (Brush)new BrushConverter().ConvertFromString("#E8F5E9"),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(6, 1, 6, 1),
                    Margin = new Thickness(8, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                var txtStock = new TextBlock
                {
                    Text = $"{p.Stock} unidades",
                    Foreground = (Brush)new BrushConverter().ConvertFromString("#43A047"),
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 11
                };
                badgeBorder.Child = txtStock;

                headerStack.Children.Add(txtNombre);
                headerStack.Children.Add(badgeBorder);

                Grid.SetRow(headerStack, 0);
                grid.Children.Add(headerStack);

                // ProgressBar que se estira correctamente
                var progress = new ProgressBar
                {
                    Value = p.Stock,
                    Maximum = MaximoProgressBar,
                    Height = 8,
                    Style = (Style)FindResource("ModernProgressBar"),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(16, 10, 16, 0),
                    MinWidth = 0,
                    Width = double.NaN // Permite que se estire
                };
                Grid.SetRow(progress, 1);
                grid.Children.Add(progress);

                border.Child = grid;
                panelProductos.Children.Add(border);
            }
        }

        private async void btnStockCompleto_Click(object sender, RoutedEventArgs e)
        {
            StockWindow stock = new StockWindow();

            // Suscribirse al evento
            stock.ProductoModificado += async () =>
            {
                await ProductosVM.CargarProductosAsync();
                MostrarProductosEnStock();
                await ActualizarNotificaciones(); // Actualizar notificaciones
            };

            stock.ShowDialog();
            await ProductosVM.CargarProductosAsync();
            MostrarProductosEnStock();
        }

        private void Filtrar_Click(object sender, RoutedEventArgs e)
        {
            var filtroWindow = new FiltroStockWindow(MaximoProgressBar);
            bool? resultado = filtroWindow.ShowDialog();

            if (resultado == true)
            {
                // Actualizamos el límite
                MaximoProgressBar = filtroWindow.Maximo;
                MostrarProductosEnStock(); // refrescamos el panel
            }
        }

        private async void TarjetaEnvios_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // RoleID 3 = Admin, ajusta segun tu logica
            bool esAdmin = RoleID == 3;
            EnviosPendientesWindow enviosWindow = new EnviosPendientesWindow(UsuarioID, esAdmin);
            enviosWindow.ShowDialog();
            await ProductosVM.CargarProductosAsync();
            MostrarProductosEnStock();
            _ = ActualizarNotificaciones();
        }

        private async void TarjetaPedidos_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MovimientosStockWindow w = new MovimientosStockWindow();
            w.Owner = this;
            w.ShowDialog();
            await ProductosVM.CargarProductosAsync();
            MostrarProductosEnStock();
        }

        private void btnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            // Detener timer de notificaciones
            if (timerNotificaciones != null)
            {
                timerNotificaciones.Stop();
            }

            MainWindow m1 = new MainWindow();
            m1.Show();
            this.Close();
        }

        private void btnAyuntamientos_Click(object sender, RoutedEventArgs e)
        {
            AyuntamientosWindow ayuntamientosWindow = new AyuntamientosWindow();
            ayuntamientosWindow.ShowDialog(); 
        }

        private void btnCategorias_Click(object sender, RoutedEventArgs e)
        {
            CategoriasWindow categoriasWindow = new CategoriasWindow();
            categoriasWindow.ShowDialog();
        }

        private void btnAjustes_Click(object sender, RoutedEventArgs e)
        {
            AjustesWindow ajustesWindow = new AjustesWindow();
            ajustesWindow.ShowDialog();
        }

        private void btnReportes_Click(object sender, RoutedEventArgs e)
        {
            ReportesWindow reportesWindow = new ReportesWindow();
            reportesWindow.ShowDialog();
        }

        private void btnUsuarios_Click(object sender, RoutedEventArgs e)
        {
            UsuariosWindow usuariosWindow = new UsuariosWindow();
            usuariosWindow.ShowDialog();
        }

        private void btnSIMs_Click(object sender, RoutedEventArgs e)
        {
            GestionSIMsWindow simsWindow = new GestionSIMsWindow();
            simsWindow.ShowDialog();
        }

        private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            // Detener timer de notificaciones
            if (timerNotificaciones != null)
            {
                timerNotificaciones.Stop();
            }

            this.Close();
        }

        private void MaximizeWindow_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }

        // Limpiar timer al cerrar ventana
        protected override void OnClosed(EventArgs e)
        {
            if (timerNotificaciones != null)
            {
                timerNotificaciones.Stop();
                timerNotificaciones = null;
            }
            base.OnClosed(e);
        }
    }
}
