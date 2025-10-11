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
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp1.Services;

namespace WpfApp1
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool temaAlternativo = true;
        private readonly IPersonaService _personaService = new PersonaService();
       

        public MainWindow()
        {
            InitializeComponent();
            ApplyTheme("ThemeLight.xaml");
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

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string nombre = txtUser.Text;
            string password = txtPassword.Password;

            bool loginExitoso = await _personaService.LoginAsync(nombre, password);

            if (loginExitoso)
            {
                MessageBox.Show("Login correcto");
                // Pasamos el nombre a HomeWindow
                HomeWindow hw = new HomeWindow(nombre);
                hw.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Usuario o contraseña incorrectos", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ForgotPassword_Click(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Función de recuperación de contraseña aún no implementada.");
        }
    }
}
