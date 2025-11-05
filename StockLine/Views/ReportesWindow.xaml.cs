using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WpfApp1.Services;

namespace WpfApp1.Views
{
    /// <summary>
    /// Interaction logic for ReportesWindow.xaml
    /// </summary>
    public partial class ReportesWindow : Window
    {
        private readonly IProductoService _productoService;
        private readonly IEnvioService _envioService;
        private readonly ISIMService _simService;
        private readonly IAyuntamientoService _ayuntamientoService;

        public ReportesWindow()
        {
            InitializeComponent();

            _productoService = new ProductoService();
            _envioService = new EnvioService();
            _simService = new SIMService();
            _ayuntamientoService = new AyuntamientoService();

            this.Loaded += ReportesWindow_Loaded;
        }

        private async void ReportesWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarEstadisticas();
        }

        private async System.Threading.Tasks.Task CargarEstadisticas()
        {
            try
            {
                var productos = await _productoService.GetAllAsync();
                var envios = await _envioService.GetAllAsync();
                var sims = await _simService.GetAllAsync();
                var ayuntamientos = await _ayuntamientoService.GetAllAsync();

                txtTotalProductos.Text = productos.Count.ToString();
                txtTotalEnvios.Text = envios.Count.ToString();
                txtTotalSIMs.Text = sims.Count.ToString();
                txtTotalAyuntamientos.Text = ayuntamientos.Count.ToString();

                var pendientes = envios.Count(e => e.Estado == "Pendiente");
                var preparados = envios.Count(e => e.Estado == "Preparado");
                var enviados = envios.Count(e => e.Estado == "Enviado");
                var totalEnvios = envios.Count > 0 ? envios.Count : 1;

                txtEnviosPendientes.Text = pendientes.ToString();
                txtEnviosPreparados.Text = preparados.ToString();
                txtEnviosEnviados.Text = enviados.ToString();

                pbPendientes.Maximum = totalEnvios;
                pbPendientes.Value = pendientes;

                pbPreparados.Maximum = totalEnvios;
                pbPreparados.Value = preparados;

                pbEnviados.Maximum = totalEnvios;
                pbEnviados.Value = enviados;

                var stockBajo = productos.Count(p => p.Stock > 0 && p.Stock < 10);
                var stockMedio = productos.Count(p => p.Stock >= 10 && p.Stock < 50);
                var stockAlto = productos.Count(p => p.Stock >= 50);

                txtStockBajo.Text = stockBajo + " productos";
                txtStockMedio.Text = stockMedio + " productos";
                txtStockAlto.Text = stockAlto + " productos";

                var simsDisponibles = sims.Count(s => s.ProductoID == null || s.ProductoID == 0);
                var simsAsignadas = sims.Count(s => s.ProductoID != null && s.ProductoID > 0);

                txtSIMsDisponibles.Text = simsDisponibles.ToString();
                txtSIMsAsignadas.Text = simsAsignadas.ToString();

                var productosEnviados = new Dictionary<int, int>();

                foreach (var envio in envios)
                {
                    if (envio.Detalles != null)
                    {
                        foreach (var detalle in envio.Detalles)
                        {
                            if (productosEnviados.ContainsKey(detalle.ProductoID))
                            {
                                productosEnviados[detalle.ProductoID] += detalle.Cantidad;
                            }
                            else
                            {
                                productosEnviados[detalle.ProductoID] = detalle.Cantidad;
                            }
                        }
                    }
                }

                var topProductos = productosEnviados
                    .OrderByDescending(p => p.Value)
                    .Take(5)
                    .Select((p, index) => new TopProductoViewModel
                    {
                        Posicion = index + 1,
                        Nombre = productos.FirstOrDefault(prod => prod.ProductoID == p.Key)?.Nombre ?? "Desconocido",
                        Cantidad = p.Value
                    })
                    .ToList();

                dgTopProductos.ItemsSource = topProductos;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar estadisticas: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnExportarPDF_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Funcionalidad de exportar a PDF en desarrollo", "Informacion", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Funcionalidad de exportar a Excel en desarrollo", "Informacion", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void TarjetaProductos_Click(object sender, MouseButtonEventArgs e)
        {
            this.Close();
            StockWindow stockWindow = new StockWindow();
            stockWindow.ShowDialog();
        }

        private void TarjetaEnvios_Click(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Redirigiendo a la seccion de Envios", "Navegacion", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void TarjetaSIMs_Click(object sender, MouseButtonEventArgs e)
        {
            this.Close();
            GestionSIMsWindow simsWindow = new GestionSIMsWindow();
            simsWindow.ShowDialog();
        }

        private void TarjetaAyuntamientos_Click(object sender, MouseButtonEventArgs e)
        {
            this.Close();
            AyuntamientosWindow ayuntamientosWindow = new AyuntamientosWindow();
            ayuntamientosWindow.ShowDialog();
        }

        private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MaximizeWindow_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }
    }

    public class TopProductoViewModel
    {
        public int Posicion { get; set; }
        public string Nombre { get; set; }
        public int Cantidad { get; set; }
    }
}
