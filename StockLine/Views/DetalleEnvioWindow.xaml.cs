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
                var resultado = false;
                string errorMsg = "No se pudo actualizar el estado.\nVerifica que el estado sea valido en la base de datos.";
                try
                {
                    resultado = await envioService.UpdateEstadoAsync(envio.EnvioID, estadoSeleccionado, usuarioId);
                }
                catch (System.Net.Http.HttpRequestException httpEx)
                {
                    
                    if (httpEx.Data["StatusCode"] != null && httpEx.Data["StatusCode"].ToString() == "409")
                    {
                        
                        if (httpEx.Data["ApiMessage"] != null)
                        {
                            errorMsg = httpEx.Data["ApiMessage"].ToString();
                        }
                    }
                    else
                    {
                        errorMsg = httpEx.Message;
                    }
                }

                if (resultado)
                {
                    MessageBox.Show("Estado actualizado correctamente.", "Exito", MessageBoxButton.OK, MessageBoxImage.Information);
                    EnvioModificado?.Invoke();
                    CargarDetalleEnvio(envio.EnvioID);
                }
                else
                {
                    
                    try
                    {
                        using (var client = new System.Net.Http.HttpClient())
                        {
                            client.BaseAddress = new Uri("http://localhost:5200/");
                            var url = $"api/envios/{envio.EnvioID}/estado?usuarioModificadorId={usuarioId}";
                            var json = Newtonsoft.Json.JsonConvert.SerializeObject(estadoSeleccionado);
                            var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
                            var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("PATCH"), url)
                            {
                                Content = content
                            };
                            var response = await client.SendAsync(request);
                            var body = await response.Content.ReadAsStringAsync();
                            if ((int)response.StatusCode == 409 && !string.IsNullOrWhiteSpace(body))
                            {
                                try
                                {
                                    var errorObj = Newtonsoft.Json.Linq.JObject.Parse(body);
                                    if (errorObj["message"] != null)
                                        errorMsg = errorObj["message"].ToString();
                                }
                                catch { }
                            }
                        }
                    }
                    catch { }
                    MessageBox.Show(errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
