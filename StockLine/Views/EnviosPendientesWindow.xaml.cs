using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.DTOs;
using WpfApp1.Services;

namespace WpfApp1.Views
{
    public partial class EnviosPendientesWindow : Window
    {
        private readonly IEnvioService _envioService;
        private readonly IAyuntamientoService _ayuntamientoService;
        private ObservableCollection<EnvioDTO> _envios;
        private ObservableCollection<EnvioDTO> _enviosFiltrados;
        private List<AyuntamientoDTO> _ayuntamientos;
        
        private int _usuarioId;
        private bool _esAdmin;

        public EnviosPendientesWindow(int usuarioId = 1, bool esAdmin = true)
        {
            InitializeComponent();
            
            _envioService = new EnvioService();
            _ayuntamientoService = new AyuntamientoService();
            _usuarioId = usuarioId;
            _esAdmin = esAdmin;
            
            _envios = new ObservableCollection<EnvioDTO>();
            _enviosFiltrados = new ObservableCollection<EnvioDTO>();
            
            this.Loaded += EnviosPendientesWindow_Loaded;
            
            if (!_esAdmin)
            {
                this.Title = "Envios Pendientes (Solo Lectura)";
            }
        }

        private async void EnviosPendientesWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarDatos();
        }

        private async System.Threading.Tasks.Task CargarDatos()
        {
            try
            {
                var envios = await _envioService.GetAllAsync();
                _envios.Clear();
                foreach (var envio in envios)
                {
                    _envios.Add(envio);
                }

                _ayuntamientos = await _ayuntamientoService.GetAllAsync();
                var nombresAyuntamientos = _ayuntamientos.Select(a => a.Nombre).OrderBy(n => n).ToList();
                nombresAyuntamientos.Insert(0, "Todos");
                cbAyuntamiento.ItemsSource = nombresAyuntamientos;
                cbAyuntamiento.SelectedIndex = 0;

                if (cbEstado != null) cbEstado.SelectedIndex = 0;
                if (cbUltimosEnvios != null) cbUltimosEnvios.SelectedIndex = 0;

                AplicarFiltros();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AplicarFiltros()
        {
            var resultado = _envios.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                var busqueda = txtSearch.Text.ToLower();
                resultado = resultado.Where(envio =>
                    envio.AyuntamientoNombre.ToLower().Contains(busqueda) ||
                    envio.EnvioID.ToString().Contains(busqueda) ||
                    (envio.Detalles != null && envio.Detalles.Any(d => d.ProductoNombre.ToLower().Contains(busqueda)))
                );
            }

            _enviosFiltrados.Clear();
            foreach (var envio in resultado)
            {
                _enviosFiltrados.Add(envio);
            }

            EnviosGrid.ItemsSource = _enviosFiltrados;
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            AplicarFiltros();
        }

        private void AplicarEstado_Click(object sender, RoutedEventArgs args)
        {
            if (cbEstado.SelectedIndex == 0)
            {
                AplicarFiltros();
                return;
            }

            var estadoSeleccionado = (cbEstado.SelectedItem as ComboBoxItem)?.Content.ToString();
            var resultado = _envios.Where(envio => envio.Estado == estadoSeleccionado);

            _enviosFiltrados.Clear();
            foreach (var envio in resultado)
            {
                _enviosFiltrados.Add(envio);
            }

            EnviosGrid.ItemsSource = _enviosFiltrados;
        }

        private void AplicarUltimosEnvios_Click(object sender, RoutedEventArgs args)
        {
            var cantidad = 10;
            if (cbUltimosEnvios.SelectedItem is ComboBoxItem item)
            {
                int.TryParse(item.Content.ToString(), out cantidad);
            }

            var resultado = _envios.OrderByDescending(envio => envio.FechaEnvio).Take(cantidad);

            _enviosFiltrados.Clear();
            foreach (var envio in resultado)
            {
                _enviosFiltrados.Add(envio);
            }

            EnviosGrid.ItemsSource = _enviosFiltrados;
        }

        private void AplicarAyuntamiento_Click(object sender, RoutedEventArgs args)
        {
            var ayuntamientoSeleccionado = cbAyuntamiento.SelectedItem?.ToString();

            if (ayuntamientoSeleccionado == "Todos" || string.IsNullOrEmpty(ayuntamientoSeleccionado))
            {
                AplicarFiltros();
                return;
            }

            var resultado = _envios.Where(envio => envio.AyuntamientoNombre == ayuntamientoSeleccionado);

            _enviosFiltrados.Clear();
            foreach (var envio in resultado)
            {
                _enviosFiltrados.Add(envio);
            }

            EnviosGrid.ItemsSource = _enviosFiltrados;
        }

        private void AplicarTodos_Click(object sender, RoutedEventArgs args)
        {
            txtSearch.Text = string.Empty;
            if (cbEstado != null) cbEstado.SelectedIndex = 0;
            if (cbUltimosEnvios != null) cbUltimosEnvios.SelectedIndex = 0;
            if (cbAyuntamiento != null) cbAyuntamiento.SelectedIndex = 0;
            
            AplicarFiltros();
        }

        private void EnviosGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var envioSeleccionado = EnviosGrid.SelectedItem as EnvioDTO;
            
            if (envioSeleccionado != null)
            {
                AbrirDetalleEnvio(envioSeleccionado);
            }
        }

        private void Editar_Click(object sender, RoutedEventArgs args)
        {
            var envioSeleccionado = EnviosGrid.SelectedItem as EnvioDTO;
            
            if (envioSeleccionado == null)
            {
                MessageBox.Show("Selecciona un envio primero.", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AbrirDetalleEnvio(envioSeleccionado);
        }

        private void AbrirDetalleEnvio(EnvioDTO envio)
        {
            var ventanaDetalle = new DetalleEnvioWindow(envio.EnvioID, _usuarioId, _esAdmin);
            ventanaDetalle.EnvioModificado += async () => await CargarDatos();
            ventanaDetalle.ShowDialog();
        }

        private async Task ActualizarSIMsUbicacionPorEnvio(EnvioDTO envio)
        {
            var simService = new SIMService();
            foreach (var detalle in envio.Detalles)
            {
                if (detalle.SIMID.HasValue)
                {
                    var sim = await simService.GetByIdAsync(detalle.SIMID.Value);
                    if (sim != null)
                    {
                        sim.Ubicacion = envio.AyuntamientoNombre;
                        await simService.UpdateAsync(sim.SIMID, sim);
                    }
                }
            }
        }

        private async void Finalizar_Click(object sender, RoutedEventArgs args)
        {
            var envioSeleccionado = EnviosGrid.SelectedItem as EnvioDTO;
            if (envioSeleccionado == null)
            {
                MessageBox.Show("Selecciona un envio primero.", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!_esAdmin)
            {
                MessageBox.Show("No tienes permisos para finalizar envios.\n\nSolo los administradores pueden realizar esta accion.", 
                    "Acceso Denegado", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning);
                return;
            }

            if (envioSeleccionado.Estado == "Enviado")
            {
                MessageBox.Show("Este envio ya está marcado como enviado.", "Informacion", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var confirmacion = MessageBox.Show(
                $"Estas seguro de marcar el envio #{envioSeleccionado.EnvioID} como Enviado?\n\n" +
                $"Ayuntamiento: {envioSeleccionado.AyuntamientoNombre}\n" +
                $"Fecha: {envioSeleccionado.FechaEnvio:dd/MM/yyyy}",
                "Confirmar Envio",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmacion != MessageBoxResult.Yes)
                return;

            try
            {
                var resultado = await _envioService.UpdateEstadoAsync(envioSeleccionado.EnvioID, "Enviado", _usuarioId);

                if (resultado)
                {
                    // Recargar datos de SIMs desde el backend
                    var simService = new SIMService();
                    var simsActualizadas = await simService.GetAllAsync();
                    // Aquí puedes actualizar la UI donde se muestran las SIMs, por ejemplo:
                    // ActualizarSIMsEnUI(simsActualizadas);
                    MessageBox.Show("Envio marcado como enviado correctamente.", "Exito", MessageBoxButton.OK, MessageBoxImage.Information);
                    await CargarDatos();
                }
                else
                {
                    MessageBox.Show("No se pudo actualizar el envio.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar el envio:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Eliminar_Click(object sender, RoutedEventArgs args)
        {
            var envioSeleccionado = EnviosGrid.SelectedItem as EnvioDTO;
            
            if (envioSeleccionado == null)
            {
                MessageBox.Show("Selecciona un envio primero.", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!_esAdmin)
            {
                MessageBox.Show("No tienes permisos para eliminar envios.\n\nSolo los administradores pueden realizar esta accion.", 
                    "Acceso Denegado", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning);
                return;
            }

            var confirmacion = MessageBox.Show(
                $"ATENCION: Estas a punto de eliminar el envio #{envioSeleccionado.EnvioID}\n\n" +
                $"Ayuntamiento: {envioSeleccionado.AyuntamientoNombre}\n" +
                $"Fecha: {envioSeleccionado.FechaEnvio:dd/MM/yyyy}\n" +
                $"Estado: {envioSeleccionado.Estado}\n\n" +
                $"Esta acion NO se puede deshacer.\n\n" +
                $"¿Deseas continuar?",
                "Confirmar Eliminacion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmacion != MessageBoxResult.Yes)
                return;

            try
            {
                var resultado = await _envioService.DeleteAsync(envioSeleccionado.EnvioID);

                if (resultado)
                {
                    MessageBox.Show("Envio eliminado correctamente.", "Exito", MessageBoxButton.OK, MessageBoxImage.Information);
                    await CargarDatos();
                }
                else
                {
                    MessageBox.Show("No se pudo eliminar el envio.\nIntenta nuevamente.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar el envio:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AñadirEnvio_Click(object sender, RoutedEventArgs args)
        {
            var ventanaCrear = new CrearEnvioWindow(_usuarioId);
            ventanaCrear.EnvioCreado += async () => await CargarDatos();
            ventanaCrear.ShowDialog();
        }

        private void ExportarCsv_Click(object sender, RoutedEventArgs args)
        {
            if (_enviosFiltrados.Count == 0)
            {
                MessageBox.Show("No hay datos para exportar.", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                FileName = $"Envios_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveFileDialog.ShowDialog() != true)
                return;

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("ID,Ayuntamiento,Comercial,Fecha Envio,Estado,Referencia,Modificado Por,Fecha Modificacion");

                foreach (var envio in _enviosFiltrados)
                {
                    sb.AppendLine($"{envio.EnvioID}," +
                                 $"\"{envio.AyuntamientoNombre}\"," +
                                 $"\"{envio.ComercialNombre ?? "N/A"}\"," +
                                 $"{envio.FechaEnvio:yyyy-MM-dd HH:mm}," +
                                 $"\"{envio.Estado}\"," +
                                 $"\"{envio.NumeroReferencia ?? "N/A"}\"," +
                                 $"\"{envio.UsuarioModificadorNombre ?? "N/A"}\"," +
                                 $"{(envio.FechaModificacion.HasValue ? envio.FechaModificacion.Value.ToString("yyyy-MM-dd HH:mm") : "N/A")}");
                }

                File.WriteAllText(saveFileDialog.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show($"Archivo exportado correctamente.\n\nUbicacion: {saveFileDialog.FileName}", "Exito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void VolverAlInicio_Click(object sender, RoutedEventArgs args)
        {
            this.Close();
        }

        private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}

