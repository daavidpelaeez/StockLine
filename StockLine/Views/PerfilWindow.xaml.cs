using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfApp1.ViewModels;
using WpfApp1;

namespace WpfApp1.Views
{
   
    public partial class PerfilWindow : Window
    {
        public PerfilWindow()
        {
            InitializeComponent();
            var vm = new PerfilViewModel();
            vm.RoleID = Session.RoleID;
            vm.Nombre = Session.NombreUsuario;
            this.DataContext = vm;
        }

        private void PasswordActualBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is PerfilViewModel vm && sender is PasswordBox pb)
                vm.PasswordActual = pb.Password;
        }
        private void NuevaPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is PerfilViewModel vm && sender is PasswordBox pb)
                vm.NuevaPassword = pb.Password;
        }
        private void ConfirmarPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is PerfilViewModel vm && sender is PasswordBox pb)
                vm.ConfirmarPassword = pb.Password;
        }

       
        private void CustomTitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                BtnMaximizar_Click(sender, e);
            }
            else
            {
                DragMove();
            }
        }

       
        private void BtnMaximizar_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }


        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
