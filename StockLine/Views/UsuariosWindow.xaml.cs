using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfApp1.DTOs;
using WpfApp1.Services;

namespace WpfApp1.Views
{
    /// <summary>
    /// Interaction logic for UsuariosWindow.xaml
    /// </summary>
    public partial class UsuariosWindow : Window
    {
        private readonly IPersonaService _personaService;
        private ObservableCollection<UsuarioViewModel> _usuarios;
        private ObservableCollection<UsuarioViewModel> _usuariosFiltrados;

        public UsuariosWindow()
        {
            InitializeComponent();

            _personaService = new PersonaService();
            _usuarios = new ObservableCollection<UsuarioViewModel>();
            _usuariosFiltrados = new ObservableCollection<UsuarioViewModel>();

            dgUsuarios.ItemsSource = _usuariosFiltrados;

            this.Loaded += UsuariosWindow_Loaded;
        }

        private async void UsuariosWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarUsuarios();
        }

        private async System.Threading.Tasks.Task CargarUsuarios()
        {
            try
            {
                var usuarios = await _personaService.GetAllAsync();
                _usuarios.Clear();

                foreach (var usuario in usuarios)
                {
                    _usuarios.Add(new UsuarioViewModel
                    {
                        UsuarioID = usuario.UsuarioID,
                        Nombre = usuario.Nombre,
                        Apellidos = usuario.Apellidos,
                        Email = usuario.Email,
                        RoleID = usuario.RoleID,
                        Activo = usuario.Activo,
                        NombreCompleto = usuario.Nombre + " " + usuario.Apellidos,
                        InicialNombre = string.IsNullOrEmpty(usuario.Nombre) ? "?" : usuario.Nombre.Substring(0, 1).ToUpper(),
                        RolNombre = ObtenerNombreRol(usuario.RoleID),
                        RolColor = ObtenerColorRol(usuario.RoleID)
                    });
                }

                AplicarFiltros();
                ActualizarEstadisticas();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar usuarios: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AplicarFiltros()
        {
            var resultado = _usuarios.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(txtBuscar.Text))
            {
                var busqueda = txtBuscar.Text.ToLower();
                resultado = resultado.Where(u =>
                    u.NombreCompleto.ToLower().Contains(busqueda) ||
                    u.Email.ToLower().Contains(busqueda) ||
                    u.RolNombre.ToLower().Contains(busqueda)
                );
            }

            _usuariosFiltrados.Clear();
            foreach (var usuario in resultado)
            {
                _usuariosFiltrados.Add(usuario);
            }
        }

        private void ActualizarEstadisticas()
        {
            txtTotalUsuarios.Text = "Total de usuarios: " + _usuariosFiltrados.Count;
            txtTotalAdmin.Text = _usuarios.Count(u => u.RoleID == 3).ToString();
            txtTotalComercial.Text = _usuarios.Count(u => u.RoleID == 2).ToString();
        }

        private string ObtenerNombreRol(int roleId)
        {
            switch (roleId)
            {
                case 1:
                    return "Usuario";
                case 2:
                    return "Comercial";
                case 3:
                    return "Admin";
                default:
                    return "Desconocido";
            }
        }

        private Brush ObtenerColorRol(int roleId)
        {
            switch (roleId)
            {
                case 1:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#95A5A6"));
                case 2:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ECC71"));
                case 3:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C"));
                default:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F8C8D"));
            }
        }

        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            AplicarFiltros();
            ActualizarEstadisticas();
        }

        private void BtnNuevoUsuario_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new CrearEditarUsuarioWindow();
            ventana.UsuarioGuardado += async () => await CargarUsuarios();
            ventana.ShowDialog();
        }

        private void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            CargarUsuarios();
        }

        private void BtnVer_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var usuario = button?.Tag as UsuarioViewModel;

            if (usuario == null)
            {
                MessageBox.Show("Selecciona un usuario", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var detalles = "Informacion del Usuario:\n\n" +
                          "ID: " + usuario.UsuarioID + "\n" +
                          "Nombre: " + usuario.NombreCompleto + "\n" +
                          "Email: " + usuario.Email + "\n" +
                          "Rol: " + usuario.RolNombre;

            MessageBox.Show(detalles, "Detalles del Usuario", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var usuario = button?.Tag as UsuarioViewModel;

            if (usuario == null)
            {
                MessageBox.Show("Selecciona un usuario para editar", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var ventana = new CrearEditarUsuarioWindow(usuario);
            ventana.UsuarioGuardado += async () => await CargarUsuarios();
            ventana.ShowDialog();
        }

        private async void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var usuario = button?.Tag as UsuarioViewModel;

            if (usuario == null)
            {
                MessageBox.Show("Selecciona un usuario para eliminar", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirmacion = MessageBox.Show(
                "Estas seguro de eliminar al usuario '" + usuario.NombreCompleto + "'?\n\n" +
                "ADVERTENCIA: No se podra eliminar si tiene movimientos asociados.\n\n" +
                "Esta accion NO se puede deshacer.",
                "Confirmar Eliminacion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmacion != MessageBoxResult.Yes)
                return;

            try
            {
                var resultado = await _personaService.DeleteAsync(usuario.UsuarioID);

                if (resultado)
                {
                    MessageBox.Show(
                        "Usuario eliminado correctamente", 
                        "Exito", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                    await CargarUsuarios();
                }
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("movimientos de stock") || 
                    ex.Message.Contains("REFERENCE constraint") ||
                    ex.Message.Contains("MovimientosStock"))
                {
                    MessageBox.Show(
                        "No se puede eliminar este usuario\n\n" +
                        "Tiene movimientos de stock asociados.\n\n" +
                        "Soluciones:\n" +
                        "- Desactivar el usuario\n" +
                        "- Reasignar los movimientos a otro usuario\n" +
                        "- Contactar con el administrador",
                        "Usuario con Registros Asociados",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show(
                        "Error: " + ex.Message,
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error inesperado: " + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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

    public class UsuarioViewModel
    {
        public int UsuarioID { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string Email { get; set; }
        public int RoleID { get; set; }
        public bool Activo { get; set; }
        public string NombreCompleto { get; set; }
        public string InicialNombre { get; set; }
        public string RolNombre { get; set; }
        public Brush RolColor { get; set; }
        public string EstadoTexto => Activo ? "Activo" : "Inactivo";
        public Brush EstadoColor => Activo ? 
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27AE60")) : 
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C"));
    }
}
