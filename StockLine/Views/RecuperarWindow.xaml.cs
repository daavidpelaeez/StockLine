using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace WpfApp1.Views
{
    /// <summary>
    /// Interaction logic for RecuperarWindow.xaml
    /// </summary>
    public partial class RecuperarWindow : Window
    {
        public RecuperarWindow()
        {
            InitializeComponent();
        }

        private async void BtnEnviar_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text.Trim();

            // Validar que no esté vacío
            if (string.IsNullOrWhiteSpace(email))
            {
                MostrarMensaje("❌", "Por favor, ingresa tu correo electronico", "#FFEBEE", "#EF5350", "#C62828");
                txtEmail.Focus();
                return;
            }

            // Validar formato de email
            if (!EsEmailValido(email))
            {
                MostrarMensaje("❌", "El formato del correo electronico no es valido", "#FFEBEE", "#EF5350", "#C62828");
                txtEmail.Focus();
                return;
            }

            try
            {
                // Deshabilitar botones durante el proceso
                btnEnviar.IsEnabled = false;
                btnCancelar.IsEnabled = false;
                btnEnviar.Content = "Enviando...";

                // Simular el envío del correo (aquí deberías llamar a tu API)
                await Task.Delay(2000); // Simulación de espera

                // En un escenario real, aquí harías:
                // await _recuperarPasswordService.EnviarEmailRecuperacion(email);

                // Mostrar mensaje de éxito
                MostrarMensaje(
                    "✅", 
                    "Correo enviado! Revisa tu bandeja de entrada", 
                    "#E8F5E9", 
                    "#66BB6A", 
                    "#27AE60");

                // Esperar un momento y cerrar
                await Task.Delay(2500);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MostrarMensaje(
                    "⚠️", 
                    "Error al enviar el correo: " + ex.Message, 
                    "#FFF3E0", 
                    "#FFA726", 
                    "#F57C00");
            }
            finally
            {
                btnEnviar.IsEnabled = true;
                btnCancelar.IsEnabled = true;
                btnEnviar.Content = "Enviar Enlace";
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private bool EsEmailValido(string email)
        {
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        private void MostrarMensaje(string icono, string mensaje, string colorFondo, string colorBorde, string colorTexto)
        {
            txtIconoMensaje.Text = icono;
            txtMensaje.Text = mensaje;
            borderMensaje.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorFondo));
            borderMensaje.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorBorde));
            txtMensaje.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorTexto));
            borderMensaje.Visibility = Visibility.Visible;
        }
    }
}
