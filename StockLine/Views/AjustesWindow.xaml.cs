using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace WpfApp1.Views
{

    public partial class AjustesWindow : Window
    {
        public AjustesWindow()
        {
            InitializeComponent();
            
        }

        

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnAcercaDe_Click(object sender, RoutedEventArgs e)
        {
            var mensaje = "╔══════════════════════════════════════════════╗\n" +
                         "║           STOCKLINE v1.0.0                   ║\n" +
                         "║    Sistema de Gestión Integral               ║\n" +
                         "╚══════════════════════════════════════════════╝\n\n" +
                         
                         "📚 PROYECTO DE 2º DAM\n" +
                         "   Desarrollo de Aplicaciones Multiplataforma\n\n" +
                         
                         "👨‍💻 DESARROLLADOR:\n" +
                         "   David Peláez\n\n" +
                         
                         "🎓 CENTRO EDUCATIVO:\n" +
                         "   IES - Ciclo Formativo de Grado Superior\n\n" +
                         
                         "📅 FECHA DE DESARROLLO:\n" +
                         "   2024 - 2025\n\n" +
                         
                         "💻 TECNOLOGÍAS UTILIZADAS:\n" +
                         "   • WPF (Windows Presentation Foundation)\n" +
                         "   • C# .NET Framework 4.8\n" +
                         "   • ASP.NET Core Web API\n" +
                         "   • Entity Framework Core\n" +
                         "   • SQL Server\n\n" +
                         
                         "🎯 FUNCIONALIDADES PRINCIPALES:\n" +
                         "   • Gestión de Stock y Productos\n" +
                         "   • Control de Envíos\n" +
                         "   • Administración de Ayuntamientos\n" +
                         "   • Gestión de Usuarios y Roles\n" +
                         "   • Gestión de SIMs\n" +
                         "   • Categorías y Proveedores\n" +
                         "   • Sistema de Reportes\n\n" +
                         
                         "📜 LICENCIA:\n" +
                         "   MIT License - Software Educativo\n\n" +
                         
                         "🌐 REPOSITORIO:\n" +
                         "   https://github.com/daavidpelaeez/StockLine\n\n" +
                         
                         "📧 CONTACTO:\n" +
                         "   Para más información sobre el proyecto\n" +
                         "   contacta con el desarrollador.\n\n" +
                         
                         "══════════════════════════════════════════════\n" +
                         "Gracias por utilizar StockLine 🚀\n" +
                         "══════════════════════════════════════════════";

            MessageBox.Show(
                mensaje,
                "Acerca de StockLine",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void Repository_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/daavidpelaeez/StockLine",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo abrir el navegador.\n\n" +
                    "URL: https://github.com/daavidpelaeez/StockLine\n\n" +
                    "Error: " + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeWindow_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }

        private void CloseWindow_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
