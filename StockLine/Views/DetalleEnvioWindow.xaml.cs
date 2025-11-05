using System;
using System.Windows;
using WpfApp1.DTOs;
using WpfApp1.Services;

namespace WpfApp1.Views
{
    public partial class DetalleEnvioWindow : Window
    {
        private readonly IEnvioService envioService;
        private readonly int usuarioId;
        private readonly bool esAdmin;
        private EnvioDTO envio;

        public event Action EnvioModificado;

        public DetalleEnvioWindow(int envioId, int usuarioActualId, bool esAdministrador)
        {
            InitializeComponent();
            envioService = new EnvioService();
            usuarioId = usuarioActualId;
            esAdmin = esAdministrador;
            
            CargarDetalleEnvio(envioId);
        }

        private async void CargarDetalleEnvio(int envioId)
        {
            try
            {
                envio = await envioService.GetByIdAsync(envioId);

                if (envio == null)
                {
                    MessageBox.Show("No se pudo cargar el detalle del envio.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                    return;
                }

                txtEnvioId.Text = $"ID: #{envio.EnvioID}";
                txtAyuntamiento.Text = envio.AyuntamientoNombre ?? "-";
                txtFechaEnvio.Text = envio.FechaEnvio.ToString("dd/MM/yyyy HH:mm");
                
                switch (envio.Estado)
                {
                    case "Pendiente":
                        cbEstado.SelectedIndex = 0;
                        break;
                    case "Preparado":
                        cbEstado.SelectedIndex = 1;
                        break;
                    case "Enviado":
                        cbEstado.SelectedIndex = 2;
                        break;
                }

                if (esAdmin)
                {
                    cbEstado.IsEnabled = true;
                    btnCambiarEstado.Visibility = Visibility.Visible;
                }

                if (envio.UsuarioModificadorID.HasValue)
                {
                    txtModificadoPor.Text = envio.UsuarioModificadorNombre ?? $"Usuario ID: {envio.UsuarioModificadorID}";
                    txtFechaModificacion.Text = envio.FechaModificacion?.ToString("dd/MM/yyyy HH:mm") ?? "-";
                }
                else
                {
                    txtModificadoPor.Text = "Sin modificaciones";
                    txtFechaModificacion.Text = "-";
                }

                dgProductos.ItemsSource = envio.Detalles;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el detalle del envio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnCambiarEstado_Click(object sender, RoutedEventArgs e)
        {
            if (!esAdmin)
            {
                MessageBox.Show("No tienes permisos para cambiar el estado.", "Acceso Denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var estadoSeleccionado = (cbEstado.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString();

            if (string.IsNullOrEmpty(estadoSeleccionado))
            {
                MessageBox.Show("Selecciona un estado valido.", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirmacion = MessageBox.Show(
                $"Estas seguro de cambiar el estado a '{estadoSeleccionado}'?",
                "Confirmar cambio",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmacion != MessageBoxResult.Yes)
                return;

            try
            {
                var resultado = await envioService.UpdateEstadoAsync(envio.EnvioID, estadoSeleccionado, usuarioId);

                if (resultado)
                {
                    MessageBox.Show("Estado actualizado correctamente.", "Exito", MessageBoxButton.OK, MessageBoxImage.Information);
                    // Crear movimientos de stock si el estado es 'Enviado'
                    if (estadoSeleccionado == "Enviado" && envio != null && envio.Detalles != null)
                    {
                        using (var client = new System.Net.Http.HttpClient())
                        {
                            client.BaseAddress = new Uri("http://localhost:5200/");
                            foreach (var detalle in envio.Detalles)
                            {
                                var movimiento = new
                                {
                                    productoID = detalle.ProductoID,
                                    cantidad = detalle.Cantidad,
                                    tipoMovimiento = "Salida",
                                    usuarioID = usuarioId,
                                    observaciones = $"Salida por Envío #{envio.EnvioID}"
                                };
                                var json = Newtonsoft.Json.JsonConvert.SerializeObject(movimiento);
                                var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
                                var resp = await client.PostAsync("api/movimientosstock", content);
                                if (!resp.IsSuccessStatusCode)
                                {
                                    var error = await resp.Content.ReadAsStringAsync();
                                    System.Diagnostics.Debug.WriteLine($"Error creando movimiento de stock: {error}");
                                    // Opcional: mostrar error al usuario
                                }
                            }
                        }
                    }
                    if (EnvioModificado != null)
                        EnvioModificado();
                    CargarDetalleEnvio(envio.EnvioID);
                }
                else
                {
                    MessageBox.Show("No se pudo actualizar el estado.\nVerifica que el estado sea valido en la base de datos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar el estado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
