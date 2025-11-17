using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WpfApp1.DTOs;
using WpfApp1.Services;
using WpfApp1.Views;

namespace WpfApp1
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool temaAlternativo = true;
        private readonly IPersonaService _personaService = new PersonaService();
        private DispatcherTimer _mensajeTimer;

        public MainWindow()
        {
            InitializeComponent();
            ApplyTheme("ThemeLight.xaml");
            
            // Timer para ocultar mensajes automáticamente
            _mensajeTimer = new DispatcherTimer();
            _mensajeTimer.Interval = TimeSpan.FromSeconds(5);
            _mensajeTimer.Tick += MensajeTimer_Tick;
        }

        private void ApplyTheme(string themeFile)
        {
            var uri = new Uri($"/Themes/{themeFile}", UriKind.Relative);
            var resourceDict = Application.LoadComponent(uri) as ResourceDictionary;

            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(resourceDict);
        }

        private void ToggleTheme_Click(object sender, RoutedEventArgs e)
        {
            if (temaAlternativo)
                ApplyTheme("ThemeDark.xaml");
            else
                ApplyTheme("ThemeLight.xaml");

            temaAlternativo = !temaAlternativo;
        }

        private bool EsEmailValido(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                    builder.Append(b.ToString("x2"));
                return builder.ToString();
            }
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string email = txtUser.Text.Trim();
            string password = txtPassword.Password;

            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(email))
            {
                MostrarMensajeError("❌", "Por favor, ingresa tu usuario o email", "#FFEBEE", "#EF5350", "#C62828");
                txtUser.Focus();
                return;
            }
            if (!EsEmailValido(email))
            {
                MostrarMensajeError("❌", "El email no tiene un formato válido", "#FFEBEE", "#EF5350", "#C62828");
                txtUser.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                MostrarMensajeError("❌", "Por favor, ingresa tu contraseña", "#FFEBEE", "#EF5350", "#C62828");
                txtPassword.Focus();
                return;
            }
            try
            {
                // Expandir tarjeta con animación
                ExpandirTarjeta();

                // Mostrar indicador de carga
                MostrarCargando(true);
                OcultarMensaje();

                // Deshabilitar controles durante el login
                btnLogin.IsEnabled = false;
                txtUser.IsEnabled = false;
                txtPassword.IsEnabled = false;

                // Simular un pequeño delay para que se vea la animación (opcional)
                await Task.Delay(800);

                // Enviar la contraseña en texto plano, la API se encarga de hashearla
                UsuarioDTO usuario = await _personaService.LoginAsync(email, password);

                if (usuario != null)
                {
                    // Login exitoso
                    MostrarCargando(false);
                    MostrarMensajeExito("✅", $"¡Bienvenido {usuario.Nombre}!", "#E8F5E9", "#66BB6A", "#27AE60");

                    // Esperar un momento para que vea el mensaje de éxito
                    await Task.Delay(1000);

                    // Abrir la ventana principal
                    HomeWindow hw = new HomeWindow(usuario.Nombre, usuario.UsuarioID, usuario.RoleID);
                    hw.Show();
                    this.Close();
                }
                else
                {
                    // Login fallido
                    MostrarCargando(false);
                    MostrarMensajeError("❌", "Usuario o contraseña incorrectos", "#FFEBEE", "#EF5350", "#C62828");
                    
                    // Limpiar contraseña
                    txtPassword.Clear();
                    txtPassword.Focus();

                    // Contraer tarjeta de vuelta
                    ContraerTarjeta();
                }
            }
            catch (Exception ex)
            {
                MostrarCargando(false);
                MostrarMensajeError("⚠️", "Error de conexión: " + ex.Message, "#FFF3E0", "#FFA726", "#F57C00");
                
                // Contraer tarjeta de vuelta
                ContraerTarjeta();
            }
            finally
            {
                // Re-habilitar controles
                btnLogin.IsEnabled = true;
                txtUser.IsEnabled = true;
                txtPassword.IsEnabled = true;
            }
        }

        private void MostrarCargando(bool mostrar)
        {
            borderCargando.Visibility = mostrar ? Visibility.Visible : Visibility.Collapsed;
        }

        private void MostrarMensajeError(string icono, string mensaje, string colorFondo, string colorBorde, string colorTexto)
        {
            txtIconoMensaje.Text = icono;
            txtMensaje.Text = mensaje;
            borderMensaje.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorFondo));
            borderMensaje.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorBorde));
            txtMensaje.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorTexto));
            borderMensaje.Visibility = Visibility.Visible;
            
            // Reiniciar timer para ocultar mensaje
            _mensajeTimer.Stop();
            _mensajeTimer.Start();
        }

        private void MostrarMensajeExito(string icono, string mensaje, string colorFondo, string colorBorde, string colorTexto)
        {
            txtIconoMensaje.Text = icono;
            txtMensaje.Text = mensaje;
            borderMensaje.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorFondo));
            borderMensaje.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorBorde));
            txtMensaje.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorTexto));
            borderMensaje.Visibility = Visibility.Visible;
        }

        private void OcultarMensaje()
        {
            borderMensaje.Visibility = Visibility.Collapsed;
            _mensajeTimer.Stop();
        }

        private void MensajeTimer_Tick(object sender, EventArgs e)
        {
            OcultarMensaje();
        }

        private void ForgotPassword_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // Crear y mostrar la ventana de recuperación de contraseña
                var recuperarWindow = new RecuperarWindow();
                recuperarWindow.Owner = this;
                recuperarWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MostrarMensajeError("⚠️", "Error al abrir recuperacion de contrasena: " + ex.Message, "#FFF3E0", "#FFA726", "#F57C00");
            }
        }

        private void ExpandirTarjeta()
        {
            var storyboard = new Storyboard();

            // Usar Height y Width actuales solo si no son NaN
            double fromHeight = double.IsNaN(loginCard.Height) ? loginCard.ActualHeight : loginCard.Height;
            double fromWidth = double.IsNaN(loginCard.Width) ? loginCard.ActualWidth : loginCard.Width;

            // Animar Height
            var heightAnimation = new DoubleAnimation
            {
                From = fromHeight,
                To = 400,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(heightAnimation, loginCard);
            Storyboard.SetTargetProperty(heightAnimation, new PropertyPath(FrameworkElement.HeightProperty));
            storyboard.Children.Add(heightAnimation);

            // Animar Width
            var widthAnimation = new DoubleAnimation
            {
                From = fromWidth,
                To = 350,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(widthAnimation, loginCard);
            Storyboard.SetTargetProperty(widthAnimation, new PropertyPath(FrameworkElement.WidthProperty));
            storyboard.Children.Add(widthAnimation);

            storyboard.Begin();
        }

        private void ContraerTarjeta()
        {
            var storyboard = new Storyboard();

            double fromHeight = double.IsNaN(loginCard.Height) ? loginCard.ActualHeight : loginCard.Height;
            double fromWidth = double.IsNaN(loginCard.Width) ? loginCard.ActualWidth : loginCard.Width;

            // Animar Height
            var heightAnimation = new DoubleAnimation
            {
                From = fromHeight,
                To = 320,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(heightAnimation, loginCard);
            Storyboard.SetTargetProperty(heightAnimation, new PropertyPath(FrameworkElement.HeightProperty));
            storyboard.Children.Add(heightAnimation);

            // Animar Width
            var widthAnimation = new DoubleAnimation
            {
                From = fromWidth,
                To = 300,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(widthAnimation, loginCard);
            Storyboard.SetTargetProperty(widthAnimation, new PropertyPath(FrameworkElement.WidthProperty));
            storyboard.Children.Add(widthAnimation);

            storyboard.Begin();
        }
    }
}
