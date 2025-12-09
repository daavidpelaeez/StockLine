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
    
    public partial class MainWindow : Window
    {
        private bool temaAlternativo = true;
        private readonly IPersonaService _personaService = new PersonaService();
        private DispatcherTimer _mensajeTimer;

        public MainWindow()
        {
            InitializeComponent();
            ApplyTheme("ThemeLight.xaml");
            
            
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


        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string email = txtUser.Text.Trim();
            string password = txtPassword.Password;

            
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
                
                ExpandirTarjeta();

               
                MostrarCargando(true);
                OcultarMensaje();

               
                btnLogin.IsEnabled = false;
                txtUser.IsEnabled = false;
                txtPassword.IsEnabled = false;

                
                await Task.Delay(800);

                
                UsuarioDTO usuario = await _personaService.LoginAsync(email, password);

                if (usuario != null)
                {
                    
                    MostrarCargando(false);
                    MostrarMensajeExito("✅", $"¡Bienvenido {usuario.Nombre}!", "#E8F5E9", "#66BB6A", "#27AE60");

                   
                    App.UsuarioIDActual = usuario.UsuarioID;

                    
                    Session.UsuarioID = usuario.UsuarioID;
                    Session.RoleID = usuario.RoleID;
                    Session.NombreUsuario = usuario.Nombre;
                    Session.ComercialID = usuario.ComercialID; 

                   
                    await Task.Delay(1000);

                  
                    HomeWindow hw = new HomeWindow(usuario.Nombre, usuario.UsuarioID, usuario.RoleID);
                    hw.Show();
                    this.Close();
                }
                else
                {
                   
                    MostrarCargando(false);
                    MostrarMensajeError("❌", "Usuario o contraseña incorrectos", "#FFEBEE", "#EF5350", "#C62828");
                    
                    
                    txtPassword.Clear();
                    txtPassword.Focus();

                    
                    ContraerTarjeta();
                }
            }
            catch (Exception ex)
            {
                MostrarCargando(false);
                MostrarMensajeError("⚠️", "Error de conexión: " + ex.Message, "#FFF3E0", "#FFA726", "#F57C00");
                
                
                ContraerTarjeta();
            }
            finally
            {
                
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
            
        }

        private void ExpandirTarjeta()
        {
            var storyboard = new Storyboard();

            
            double fromHeight = double.IsNaN(loginCard.Height) ? loginCard.ActualHeight : loginCard.Height;
            double fromWidth = double.IsNaN(loginCard.Width) ? loginCard.ActualWidth : loginCard.Width;

            
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
