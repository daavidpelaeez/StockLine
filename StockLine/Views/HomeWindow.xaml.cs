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
using WpfApp1;

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
        private const int STOCK_MINIMO = 10; 
        private NotificacionService _notificacionService;

        public event Action ProductoModificado;
        
        public HomeWindow(String usuario, int usuarioId = 1, int roleId = 1)
        {
            InitializeComponent();
            NombreUsuario = usuario;
            UsuarioID = usuarioId;
            RoleID = roleId;
            DataContext = this;

           
            Session.UsuarioID = usuarioId;
            Session.RoleID = roleId;
            Session.NombreUsuario = usuario;
            

           
            _notificacionService = new NotificacionService();
            
            
            CargarProductosHome();
            
            
            InicializarTimerNotificaciones();

            this.Loaded += HomeWindow_Loaded;
            btnInicio.Click += btnInicio_Click;
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
            timerNotificaciones.Interval = TimeSpan.FromSeconds(30); 
            timerNotificaciones.Tick += async (s, e) => await ActualizarNotificaciones();
            timerNotificaciones.Start();
            
           
            _ = ActualizarNotificaciones();
        }

        private async Task ActualizarNotificaciones()
        {
            try
            {
                int totalNotificaciones = 0;

               
                int productosStockBajo = await ObtenerProductosStockBajo();
                
                
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

                
                int enviosPendientes = await ObtenerEnviosPendientes();

               
                if (txtKpiEnviosPendientes != null)
                    txtKpiEnviosPendientes.Text = enviosPendientes.ToString();

                
                bool enviosPendientesDescartados = _notificacionService.FueDescartada("EnviosPendientes");
                
                if (enviosPendientes > 0 && !enviosPendientesDescartados)
                {
                    notifEnviosPendientes.Visibility = Visibility.Visible;
                    txtEnviosPendientes.Text = enviosPendientes == 1 
                        ? "1 envio pendiente" 
                        : $"{enviosPendientes} envios pendientes";
                    totalNotificaciones += 1;
                }
                else
                {
                    notifEnviosPendientes.Visibility = Visibility.Collapsed;
                }

                
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
                
                if (ProductosVM.ProductosFiltrados == null || !ProductosVM.ProductosFiltrados.Any())
                {
                    await ProductosVM.CargarProductosAsync();
                }

               
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
                
                var envioService = new EnvioService();
                var todosLosEnvios = await envioService.GetAllAsync();
                
                if (todosLosEnvios == null)
                {
                    return 0;
                }

                
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

                
                _notificacionService.LimpiarNotificacionesExpiradas(48);

                await ActualizarNotificaciones();
                await Task.Delay(500); 

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
            
            StockWindow stock = new StockWindow();

            stock.ProductoModificado += async () =>
            {
               
                _notificacionService.LimpiarTodasLasNotificaciones();
                
                await ProductosVM.CargarProductosAsync();
                MostrarProductosEnStock();
                await ActualizarNotificaciones(); 
            };

            stock.ShowDialog();
        }

        private void NotificacionEnviosPendientes_Click(object sender, MouseButtonEventArgs e)
        {
            
            bool esAdmin = RoleID == 3;
            EnviosPendientesWindow enviosWindow = new EnviosPendientesWindow(UsuarioID, esAdmin);
            enviosWindow.ShowDialog();
            
            
            _ = ActualizarNotificaciones();
        }

        private void DescartarStockBajo_Click(object sender, RoutedEventArgs e)
        {
            
            _notificacionService.DescarrarNotificacion("StockBajo");
            
            
            notifStockBajo.Visibility = Visibility.Collapsed;
            
            
            ActualizarBadgeNotificaciones();
        }

        private void DescartarEnviosPendientes_Click(object sender, RoutedEventArgs e)
        {
            
            _notificacionService.DescarrarNotificacion("EnviosPendientes");
            
            
            notifEnviosPendientes.Visibility = Visibility.Collapsed;
            
           
            ActualizarBadgeNotificaciones();
        }

        private void ActualizarBadgeNotificaciones()
        {
            
            int totalNotificaciones = 0;
            
            if (notifStockBajo.Visibility == Visibility.Visible)
                totalNotificaciones++;
            
            if (notifEnviosPendientes.Visibility == Visibility.Visible)
                totalNotificaciones++;
            
            
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
            await ActualizarKpiStockBajo();
            await ActualizarKpiSIMsDisponibles();
        }

        private void MostrarProductosEnStock()
        {
            panelProductos.Items.Clear();

            
            var productosFiltrados = ProductosVM.ProductosFiltrados
                            .OrderByDescending(p => p.Stock)
                            .Take(MaxProductosMostrar)
                            .ToList();

            int maxStock = MaximoProgressBar > 0 ? MaximoProgressBar : 100;
            if (maxStock < 100) maxStock = 100;

            foreach (var p in productosFiltrados)
            {
                var border = new Border
                {
                    Background = Brushes.White,
                    CornerRadius = new CornerRadius(12),
                    Padding = new Thickness(0, 10, 0, 10),
                    Margin = new Thickness(0, 0, 0, 16),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Width = double.NaN 
                };

                var stack = new StackPanel { Orientation = Orientation.Vertical };

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

                var progressGrid = new Grid { Margin = new Thickness(16, 10, 16, 0), HorizontalAlignment = HorizontalAlignment.Stretch };
                progressGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                var progress = new ProgressBar
                {
                    Value = p.Stock,
                    Maximum = maxStock,
                    Height = 8,
                    Style = (Style)FindResource("ModernProgressBar"),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = double.NaN 
                };
                Grid.SetColumn(progress, 0);
                progressGrid.Children.Add(progress);

                stack.Children.Add(headerStack);
                stack.Children.Add(progressGrid);

                border.Child = stack;
                panelProductos.Items.Add(border);
            }
        }

        private async void btnStockCompleto_Click(object sender, RoutedEventArgs e)
        {
            StockWindow stock = new StockWindow();

           
            stock.ProductoModificado += async () =>
            {
                await ProductosVM.CargarProductosAsync();
                MostrarProductosEnStock();
                await ActualizarNotificaciones(); 
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
                
                MaximoProgressBar = filtroWindow.Maximo;
                MostrarProductosEnStock(); 
            }
        }

        private async void TarjetaEnvios_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
           
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
            
            var usuariosWindow = new UsuariosWindow(RoleID == 3);
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

       
        protected override void OnClosed(EventArgs e)
        {
            if (timerNotificaciones != null)
            {
                timerNotificaciones.Stop();
                timerNotificaciones = null;
            }
            base.OnClosed(e);
        }

        private async void HomeWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await ActualizarKpiSIMsDisponibles();
            await ActualizarKpiStockBajo();
        }

        private async Task ActualizarKpiSIMsDisponibles()
        {
            var simService = new GestionSIMsService(); 
            int simsDisponibles = await simService.GetSIMsDisponiblesAsync();
            if (txtKpiSIMsDisponibles != null)
                txtKpiSIMsDisponibles.Text = simsDisponibles.ToString();
        }

        private async Task ActualizarKpiStockBajo()
        {
            await ProductosVM.CargarProductosAsync(); 
            int productosStockBajo = ProductosVM.ProductosFiltrados
                .Where(p => p.Stock < STOCK_MINIMO)
                .Count();
            if (txtKpiStockBajo != null)
                txtKpiStockBajo.Text = productosStockBajo.ToString();
        }

        private async void TarjetaSIMs_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            GestionSIMsWindow simsWindow = new GestionSIMsWindow();
            simsWindow.ShowDialog();
            await ActualizarKpiSIMsDisponibles(); 
        }

        private async void TarjetaStockBajo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            StockWindow stockWindow = new StockWindow();
            stockWindow.ShowDialog();
            await ActualizarKpiStockBajo(); 
        }

        private void btnMovimientosStock_Click(object sender, RoutedEventArgs e)
        {
            var mvsWindow = new MovimientosStockWindow();
            mvsWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            mvsWindow.ShowDialog();
        }

        private void btnInicio_Click(object sender, RoutedEventArgs e)
        {
            var perfilWindow = new PerfilWindow();
            perfilWindow.Owner = this;
            perfilWindow.ShowDialog();
        }
    }
}
