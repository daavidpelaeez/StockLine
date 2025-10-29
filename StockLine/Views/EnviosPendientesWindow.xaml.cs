using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using WpfApp1.Models;
using WpfApp1.Services;

namespace WpfApp1.Views
{
    /// <summary>
    /// Lógica de interacción para EnviosPendientesWindow.xaml
    /// </summary>
    public partial class EnviosPendientesWindow : Window
    {
        private ObservableCollection<Envio> _envios;

        public EnviosPendientesWindow()
        {
            InitializeComponent();

            // Hardcode: lista de envíos de ejemplo
            _envios = new ObservableCollection<Envio>
            {
                new Envio { Id = 1, Ayuntamiento = new Ayuntamiento { Id = 1, Nombre = "Madrid" }, Producto = new Producto { Id = 1, Nombre = "Mascarillas" }, Cantidad = 500, FechaEnvio = DateTime.Now.AddDays(-2).AddHours(9), Estado = "Pendiente" },
                new Envio { Id = 2, Ayuntamiento = new Ayuntamiento { Id = 2, Nombre = "Barcelona" }, Producto = new Producto { Id = 2, Nombre = "Gel hidroalcohólico" }, Cantidad = 240, FechaEnvio = DateTime.Now.AddDays(-7).AddHours(11), Estado = "En Proceso" },
                new Envio { Id = 3, Ayuntamiento = new Ayuntamiento { Id = 3, Nombre = "Valencia" }, Producto = new Producto { Id = 3, Nombre = "Guantes" }, Cantidad = 1200, FechaEnvio = DateTime.Now.AddDays(-3).AddHours(14), Estado = "Pendiente" },
                new Envio { Id = 4, Ayuntamiento = new Ayuntamiento { Id = 4, Nombre = "Sevilla" }, Producto = new Producto { Id = 4, Nombre = "Termómetros" }, Cantidad = 60, FechaEnvio = DateTime.Now.AddDays(-15).AddHours(10), Estado = "Finalizado" },
                new Envio { Id = 5, Ayuntamiento = new Ayuntamiento { Id = 5, Nombre = "Zaragoza" }, Producto = new Producto { Id = 5, Nombre = "Kits PCR" }, Cantidad = 40, FechaEnvio = DateTime.Now.AddDays(-1).AddHours(16), Estado = "Pendiente" },
                new Envio { Id = 6, Ayuntamiento = new Ayuntamiento { Id = 6, Nombre = "Málaga" }, Producto = new Producto { Id = 6, Nombre = "Oxímetros" }, Cantidad = 75, FechaEnvio = DateTime.Now.AddDays(-4).AddHours(9), Estado = "En Proceso" },
                new Envio { Id = 7, Ayuntamiento = new Ayuntamiento { Id = 7, Nombre = "Alicante" }, Producto = new Producto { Id = 1, Nombre = "Mascarillas" }, Cantidad = 300, FechaEnvio = DateTime.Now.AddDays(-9).AddHours(15), Estado = "Finalizado" },
                new Envio { Id = 8, Ayuntamiento = new Ayuntamiento { Id = 8, Nombre = "Bilbao" }, Producto = new Producto { Id = 3, Nombre = "Guantes" }, Cantidad = 420, FechaEnvio = DateTime.Now.AddDays(-6).AddHours(10), Estado = "Pendiente" },
                new Envio { Id = 9, Ayuntamiento = new Ayuntamiento { Id = 9, Nombre = "Granada" }, Producto = new Producto { Id = 7, Nombre = "Gafas protectoras" }, Cantidad = 150, FechaEnvio = DateTime.Now.AddDays(-11).AddHours(12), Estado = "En Proceso" },
                new Envio { Id = 10, Ayuntamiento = new Ayuntamiento { Id = 10, Nombre = "Córdoba" }, Producto = new Producto { Id = 4, Nombre = "Termómetros" }, Cantidad = 30, FechaEnvio = DateTime.Now.AddDays(-20).AddHours(13), Estado = "Finalizado" },
                new Envio { Id = 11, Ayuntamiento = new Ayuntamiento { Id = 1, Nombre = "Madrid" }, Producto = new Producto { Id = 2, Nombre = "Gel hidroalcohólico" }, Cantidad = 600, FechaEnvio = DateTime.Now.AddDays(-5).AddHours(10), Estado = "Pendiente" },
                new Envio { Id = 12, Ayuntamiento = new Ayuntamiento { Id = 2, Nombre = "Barcelona" }, Producto = new Producto { Id = 6, Nombre = "Oxímetros" }, Cantidad = 90, FechaEnvio = DateTime.Now.AddDays(-8).AddHours(9), Estado = "En Proceso" },
                new Envio { Id = 13, Ayuntamiento = new Ayuntamiento { Id = 3, Nombre = "Valencia" }, Producto = new Producto { Id = 5, Nombre = "Kits PCR" }, Cantidad = 25, FechaEnvio = DateTime.Now.AddDays(-12).AddHours(11), Estado = "Finalizado" },
                new Envio { Id = 14, Ayuntamiento = new Ayuntamiento { Id = 4, Nombre = "Sevilla" }, Producto = new Producto { Id = 1, Nombre = "Mascarillas" }, Cantidad = 800, FechaEnvio = DateTime.Now.AddDays(-1).AddHours(8), Estado = "Pendiente" },
                new Envio { Id = 15, Ayuntamiento = new Ayuntamiento { Id = 5, Nombre = "Zaragoza" }, Producto = new Producto { Id = 3, Nombre = "Guantes" }, Cantidad = 350, FechaEnvio = DateTime.Now.AddDays(-2).AddHours(9), Estado = "En Proceso" },
                new Envio { Id = 16, Ayuntamiento = new Ayuntamiento { Id = 6, Nombre = "Málaga" }, Producto = new Producto { Id = 7, Nombre = "Gafas protectoras" }, Cantidad = 45, FechaEnvio = DateTime.Now.AddDays(-18).AddHours(14), Estado = "Finalizado" },
                new Envio { Id = 17, Ayuntamiento = new Ayuntamiento { Id = 7, Nombre = "Alicante" }, Producto = new Producto { Id = 4, Nombre = "Termómetros" }, Cantidad = 20, FechaEnvio = DateTime.Now.AddDays(-3).AddHours(16), Estado = "Pendiente" },
                new Envio { Id = 18, Ayuntamiento = new Ayuntamiento { Id = 8, Nombre = "Bilbao" }, Producto = new Producto { Id = 5, Nombre = "Kits PCR" }, Cantidad = 10, FechaEnvio = DateTime.Now.AddDays(-30).AddHours(10), Estado = "Finalizado" },
                new Envio { Id = 19, Ayuntamiento = new Ayuntamiento { Id = 9, Nombre = "Granada" }, Producto = new Producto { Id = 2, Nombre = "Gel hidroalcohólico" }, Cantidad = 200, FechaEnvio = DateTime.Now.AddDays(-13).AddHours(9), Estado = "En Proceso" },
                new Envio { Id = 20, Ayuntamiento = new Ayuntamiento { Id = 10, Nombre = "Córdoba" }, Producto = new Producto { Id = 6, Nombre = "Oxímetros" }, Cantidad = 55, FechaEnvio = DateTime.Now.AddDays(-21).AddHours(11), Estado = "Finalizado" },
                new Envio { Id = 21, Ayuntamiento = new Ayuntamiento { Id = 11, Nombre = "Santander" }, Producto = new Producto { Id = 1, Nombre = "Mascarillas" }, Cantidad = 410, FechaEnvio = DateTime.Now.AddDays(-4).AddHours(10), Estado = "Pendiente" },
                new Envio { Id = 22, Ayuntamiento = new Ayuntamiento { Id = 12, Nombre = "Pamplona" }, Producto = new Producto { Id = 2, Nombre = "Gel hidroalcohólico" }, Cantidad = 330, FechaEnvio = DateTime.Now.AddDays(-6).AddHours(15), Estado = "En Proceso" },
                new Envio { Id = 23, Ayuntamiento = new Ayuntamiento { Id = 13, Nombre = "Vigo" }, Producto = new Producto { Id = 3, Nombre = "Guantes" }, Cantidad = 220, FechaEnvio = DateTime.Now.AddDays(-9).AddHours(9), Estado = "Pendiente" },
                new Envio { Id = 24, Ayuntamiento = new Ayuntamiento { Id = 14, Nombre = "Logroño" }, Producto = new Producto { Id = 4, Nombre = "Termómetros" }, Cantidad = 18, FechaEnvio = DateTime.Now.AddDays(-16).AddHours(13), Estado = "Finalizado" },
                new Envio { Id = 25, Ayuntamiento = new Ayuntamiento { Id = 15, Nombre = "Toledo" }, Producto = new Producto { Id = 5, Nombre = "Kits PCR" }, Cantidad = 12, FechaEnvio = DateTime.Now.AddDays(-2).AddHours(9), Estado = "Pendiente" }
            };

            // Asignar al DataGrid para que se muestren inmediatamente
            EnviosGrid.ItemsSource = _envios;

            // Llenar combo de ayuntamientos con valores únicos (opcional)
            var ayuntamientos = _envios.Select(e => e.Ayuntamiento?.Nombre ?? "—").Distinct().OrderBy(n => n).ToList();
            ayuntamientos.Insert(0, "Todos");
            cbAyuntamiento.ItemsSource = ayuntamientos;
            cbAyuntamiento.SelectedIndex = 0;

            // Fijar valores por defecto para otros combos (si existen en tu XAML)
            if (cbEstado != null) cbEstado.SelectedIndex = 0;
            if (cbUltimosEnvios != null) cbUltimosEnvios.SelectedIndex = 0;
        }

        //private async void CargarEnvios()
        //{
        ////    _envios = await _envioService.GetEnviosPendientesAsync();
        ////    EnviosGrid.ItemsSource = _envios;
        ////}

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

        private void TxtSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void Importar_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Finalizar_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ExportarCsv_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Eliminar_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Editar_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AplicarUltimosEnvios_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AplicarTodos_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AplicarEstado_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AplicarAyuntamiento_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SeleccionarTodos_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

