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

namespace WpfApp1
{
    /// <summary>
    /// Lógica de interacción para HomeWindow.xaml
    /// </summary>
    public partial class HomeWindow : Window
    {

        public string NombreUsuario { get; set; }
        public HomeWindow(String usuario)
        {
            InitializeComponent();
            NombreUsuario = usuario;
            DataContext = this;
        }

        private void btnStockCompleto_Click(object sender, RoutedEventArgs e)
        {

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
    }
}
