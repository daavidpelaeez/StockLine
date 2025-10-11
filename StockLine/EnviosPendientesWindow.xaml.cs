using System.Collections.Generic;
using System.Windows;
using WpfApp1.Models;
using WpfApp1.Services;

namespace WpfApp1
{
    /// <summary>
    /// Lógica de interacción para EnviosPendientesWindow.xaml
    /// </summary>
    public partial class EnviosPendientesWindow : Window
    {
        private readonly EnvioService _envioService;
        private List<Envio> _envios;

        public EnviosPendientesWindow()
        {
            InitializeComponent();
            _envioService = new EnvioService();
            CargarEnvios();
        }

        private async void CargarEnvios()
        {
            _envios = await _envioService.GetEnviosPendientesAsync();
            EnviosGrid.ItemsSource = _envios;
        }

        private void EditarEnvio_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private async void FinalizarEnvio_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private async void BorrarEnvio_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void AñadirEnvio_Click(object sender, RoutedEventArgs e)
        {
            AddEnvioPendiente ep = new AddEnvioPendiente();
            ep.Show();
        }

        private void VerFinalizados_Click(object sender, RoutedEventArgs e)
        {

        }

        private void VerPendientes_Click(object sender, RoutedEventArgs e)
        {

        }

        private void VerPorAyuntamiento_Click(object sender, RoutedEventArgs e)
        {

        }

        private void VerUltimosDiez_Click(object sender, RoutedEventArgs e)
        {

        }

        private void VolverAlInicio_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

