using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using WpfApp1.DTOs;
using WpfApp1.Services;

namespace WpfApp1.ViewModels
{
    public class ReportesViewModel : INotifyPropertyChanged
    {
        public SeriesCollection EnviosPorDiaSeries { get; set; }
        public List<string> FechasEnvios { get; set; }
        public SeriesCollection ProductosPieSeries { get; set; }
        public SeriesCollection StockPorProductoSeries { get; set; }
        public List<string> NombresProductos { get; set; }

        public ReportesViewModel()
        {
            
            _ = CargarDatosAsync();
        }

        private async Task CargarDatosAsync()
        {
            var envioService = new EnvioService();
            var productoService = new ProductoService();

            // 1. Envíos por día (últimos 7 días)
            var envios = await envioService.GetAllAsync();
            var ultimosDias = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-6 + i))
                .ToList();
            FechasEnvios = ultimosDias.Select(d => d.ToString("dd/MM")).ToList();
            var enviosPorDia = ultimosDias
                .Select(d => envios.Count(e => e.FechaEnvio.Date == d.Date))
                .ToList();
            EnviosPorDiaSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Envíos",
                    Values = new ChartValues<int>(enviosPorDia),
                    Fill = System.Windows.Media.Brushes.LightBlue,
                    Stroke = System.Windows.Media.Brushes.SteelBlue,
                    PointGeometry = null,
                    LineSmoothness = 0
                }
            };
            OnPropertyChanged(nameof(EnviosPorDiaSeries));
            OnPropertyChanged(nameof(FechasEnvios));

            // 2. Productos más enviados (pie/donut)
            var productosEnv = envios
                .SelectMany(e => e.Detalles)
                .GroupBy(d => d.ProductoNombre)
                .Select(g => new { Producto = g.Key, Total = g.Sum(x => x.Cantidad) })
                .OrderByDescending(x => x.Total)
                .Take(5)
                .ToList();
            ProductosPieSeries = new SeriesCollection();
            foreach (var p in productosEnv)
            {
                ProductosPieSeries.Add(new PieSeries { Title = p.Producto, Values = new ChartValues<int> { p.Total }, DataLabels = true });
            }
            OnPropertyChanged(nameof(ProductosPieSeries));

            // 3. Stock por producto (barras)
            var productos = await productoService.GetAllAsync();
            var topProductos = productos.OrderByDescending(p => p.Stock).Take(7).ToList();
            NombresProductos = topProductos.Select(p => p.Nombre).ToList();
            StockPorProductoSeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Stock",
                    Values = new ChartValues<int>(topProductos.Select(p => p.Stock))
                }
            };
            OnPropertyChanged(nameof(StockPorProductoSeries));
            OnPropertyChanged(nameof(NombresProductos));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
