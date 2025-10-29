using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfApp1.ViewModels;
using WpfApp1.Views;

namespace WpfApp1.Views
{
    public partial class HomeWindow : Window
    {

        public string NombreUsuario { get; set; }
        public ProductosViewModel ProductosVM { get; set; } = new ProductosViewModel();

        public int MaximoProgressBar { get; set; } = 2000;

        private int MaxProductosMostrar = 5; 


        public event Action ProductoModificado;
        public HomeWindow(String usuario)
        {
            InitializeComponent();
            NombreUsuario = usuario;
            DataContext = this;
            CargarProductosHome();
        }

        private async void CargarProductosHome()
        {
            await ProductosVM.CargarProductosAsync();
            MostrarProductosEnStock();
        }

        private void MostrarProductosEnStock()
        {
            panelProductos.Children.Clear();

            // Filtrar productos según el máximo de la ProgressBar
            var productosFiltrados = ProductosVM.ProductosFiltrados
                                        .Where(p => p.Stock <= MaximoProgressBar)
                                        .Take(MaxProductosMostrar); // limitar a X productos

            foreach (var p in productosFiltrados)
            {
                var border = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(248, 248, 248)),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(10),
                    Margin = new Thickness(0, 0, 0, 10)
                };

                var stack = new StackPanel();

                var txtNombre = new TextBlock
                {
                    Text = p.Nombre,
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 14
                };

                var txtStock = new TextBlock
                {
                    Text = $"{p.Stock} unidades",
                    FontSize = 12,
                    Foreground = p.Stock < 10 ? Brushes.Red : Brushes.Gray
                };

                var progress = new ProgressBar
                {
                    Value = p.Stock,
                    Maximum = MaximoProgressBar,
                    Height = 6,
                    Margin = new Thickness(0, 5, 0, 0)
                };

                stack.Children.Add(txtNombre);
                stack.Children.Add(txtStock);
                stack.Children.Add(progress);

                border.Child = stack;
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
            };

            stock.ShowDialog();
        }

        private void Filtrar_Click(object sender, MouseButtonEventArgs e)
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



        private void TarjetaEnvios_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            EnviosPendientesWindow enviosWindow = new EnviosPendientesWindow();
            enviosWindow.ShowDialog();
        }

        private void btnCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            MainWindow m1 = new MainWindow();
            m1.Show();
            this.Close();
        }

        private void btnAyuntamientos_Click(object sender, RoutedEventArgs e)
        {
            AyuntamientosWindow ayuntamientosWindow = new AyuntamientosWindow();
            ayuntamientosWindow.ShowDialog(); 
        }
    }
}
