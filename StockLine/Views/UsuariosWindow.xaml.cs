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
        private readonly IPersonaService _personaService = new PersonaService();
        private ObservableCollection<UsuarioViewModel> _usuarios;
        private ObservableCollection<UsuarioViewModel> _usuariosFiltrados;
        private string _filtroEstado = "Activos";

        public UsuariosWindow()
        {
            InitializeComponent();

            _usuarios = new ObservableCollection<UsuarioViewModel>();
            _usuariosFiltrados = new ObservableCollection<UsuarioViewModel>();

            dgUsuarios.ItemsSource = _usuariosFiltrados;

            this.Loaded += UsuariosWindow_Loaded;
        }

        private async void UsuariosWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarUsuariosPorFiltro();
        }

        private async System.Threading.Tasks.Task CargarUsuariosPorFiltro()
        {
            bool? soloActivos = null;
            if (_filtroEstado == "Activos") soloActivos = true;
            else if (_filtroEstado == "Inactivos") soloActivos = false;
            // Si es "Todos", soloActivos queda en null
            await CargarUsuarios(soloActivos);
        }

        private async System.Threading.Tasks.Task CargarUsuarios(bool? soloActivos)
        {
            try
            {
                if (_personaService == null)
                {
                    MessageBox.Show("Error interno: El servicio de personas no está inicializado.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                string query = null;
                if (soloActivos.HasValue)
                    query = soloActivos.Value ? "?activos=true" : "?activos=false";
                // Si soloActivos es null, query queda en null y se piden todos los usuarios
                var usuarios = await _personaService.GetAllAsync(query);
                _usuarios.Clear();
                if (usuarios != null)
                {
                    foreach (var usuario in usuarios)
                    {
                        if (usuario == null)
                            continue;
                        string nombre = usuario.Nombre ?? string.Empty;
                        string apellidos = usuario.Apellidos ?? string.Empty;
                        string email = usuario.Email ?? string.Empty;
                        _usuarios.Add(new UsuarioViewModel
                        {
                            UsuarioID = usuario.UsuarioID,
                            Nombre = nombre,
                            Apellidos = apellidos,
                            Email = email,
                            RoleID = usuario.RoleID,
                            Activo = usuario.Activo,
                            NombreCompleto = nombre + " " + apellidos,
                            InicialNombre = string.IsNullOrEmpty(nombre) ? "?" : nombre.Substring(0, 1).ToUpper(),
                            RolNombre = ObtenerNombreRol(usuario.RoleID),
                            RolColor = ObtenerColorRol(usuario.RoleID)
                        });
                    }
                }
                AplicarFiltros();
                ActualizarEstadisticas();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar usuarios: " + ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AplicarFiltros()
        {
            var resultado = _usuarios.AsEnumerable();

            // Comprobación de nulidad para txtBuscar
            if (txtBuscar != null && !string.IsNullOrWhiteSpace(txtBuscar.Text))
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
            txtTotalUsuarios.Text = $"Total de usuarios: {_usuariosFiltrados.Count}";
            txtTotalAdmin.Text = $"Admin: {_usuariosFiltrados.Count(u => u.RolNombre == "Admin")}";
            txtTotalComercial.Text = $"Comercial: {_usuariosFiltrados.Count(u => u.RolNombre == "Comercial")}";
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
            ventana.UsuarioGuardado += async () => await CargarUsuariosPorFiltro();
            ventana.ShowDialog();
        }

        private void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            CargarUsuariosPorFiltro();
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
            ventana.UsuarioGuardado += async () => await CargarUsuariosPorFiltro();
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
                    await CargarUsuariosPorFiltro();
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

        private async void CbFiltroEstado_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbFiltroEstado.SelectedItem is ComboBoxItem item)
            {
                _filtroEstado = item.Content.ToString();
                await CargarUsuariosPorFiltro();
            }
        }

        private void BtnVer_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
            {
                MessageBox.Show("Error interno: el botón no es válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var usuario = button.Tag as UsuarioViewModel;
            if (usuario == null)
            {
                MessageBox.Show("Selecciona un usuario", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrEmpty(usuario.NombreCompleto) && string.IsNullOrEmpty(usuario.Email))
            {
                MessageBox.Show("El usuario seleccionado no tiene datos válidos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var estado = usuario.Activo ? "Activo" : "Inactivo";
            var colorEstado = usuario.Activo ? "#27AE60" : "#E74C3C";
            var dialog = new Window
            {
                Title = "Detalles del Usuario",
                Width = 420,
                Height = 420,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                Background = Brushes.White,
                Owner = this,
                Content = new Border
                {
                    Background = Brushes.White,
                    CornerRadius = new CornerRadius(16),
                    Padding = new Thickness(32),
                    Effect = new System.Windows.Media.Effects.DropShadowEffect { Color = Colors.Black, BlurRadius = 30, Opacity = 0.13, ShadowDepth = 0 },
                    Child = new StackPanel
                    {
                        Orientation = Orientation.Vertical,
                        Children =
                        {
                            new TextBlock { Text = "Información del Usuario", FontSize = 20, FontWeight = FontWeights.Bold, Foreground = (Brush)new BrushConverter().ConvertFromString("#2C3E50"), Margin = new Thickness(0,0,0,18) },
                            new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0,0,0,10), Children =
                                {
                                    new Border { Width = 48, Height = 48, CornerRadius = new CornerRadius(24), Background = (Brush)new BrushConverter().ConvertFromString("#2C3E50"), Child = new TextBlock { Text = usuario.InicialNombre, Foreground = Brushes.White, FontWeight = FontWeights.Bold, FontSize = 22, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center }, VerticalAlignment = VerticalAlignment.Center },
                                    new StackPanel { Margin = new Thickness(12,0,0,0), Children =
                                        {
                                            new TextBlock { Text = usuario.NombreCompleto, FontWeight = FontWeights.Bold, FontSize = 15, Foreground = (Brush)new BrushConverter().ConvertFromString("#2C3E50") },
                                            new TextBlock { Text = usuario.Email, FontSize = 12, Foreground = (Brush)new BrushConverter().ConvertFromString("#7F8C8D"), Margin = new Thickness(0,2,0,0) }
                                        }
                                    }
                                }
                            },
                            new TextBlock { Text = $"Rol: {usuario.RolNombre}", FontSize = 14, Foreground = (Brush)new BrushConverter().ConvertFromString("#2C3E50"), Margin = new Thickness(0,8,0,0) },
                            new TextBlock { Text = $"Estado: {estado}", FontSize = 14, Foreground = (Brush)new BrushConverter().ConvertFromString(colorEstado), Margin = new Thickness(0,8,0,0) },
                            new TextBlock { Text = $"ID: {usuario.UsuarioID}", FontSize = 13, Foreground = (Brush)new BrushConverter().ConvertFromString("#7F8C8D"), Margin = new Thickness(0,8,0,0) },
                            new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0,30,0,0), HorizontalAlignment = HorizontalAlignment.Right, Children =
                                {
                                    new Button { Content = usuario.Activo ? "Desactivar" : "Activar", Width = 120, Height = 38, Margin = new Thickness(0,0,12,0), Background = usuario.Activo ? (Brush)new BrushConverter().ConvertFromString("#E74C3C") : (Brush)new BrushConverter().ConvertFromString("#27AE60"), Foreground = Brushes.White, FontWeight = FontWeights.Bold, BorderThickness = new Thickness(0), Cursor = System.Windows.Input.Cursors.Hand, Tag = usuario },
                                    new Button { Content = "Cerrar", Width = 120, Height = 38, Background = (Brush)new BrushConverter().ConvertFromString("#2C3E50"), Foreground = Brushes.White, FontWeight = FontWeights.Bold, BorderThickness = new Thickness(0), Cursor = System.Windows.Input.Cursors.Hand, IsCancel = true }
                                }
                            }
                        }
                    }
                }
            };
            // Activar/desactivar usuario
            var btnToggle = ((dialog.Content as Border).Child as StackPanel).Children[5] as StackPanel;
            var btnToggleUser = btnToggle.Children[0] as Button;
            btnToggleUser.Click += async (s, ev) =>
            {
                try
                {
                    using (var client = new System.Net.Http.HttpClient())
                    {
                        client.BaseAddress = new Uri("http://localhost:5200/");
                        var dto = new {
                            UsuarioID = usuario.UsuarioID,
                            Nombre = usuario.Nombre,
                            Apellidos = usuario.Apellidos,
                            Email = usuario.Email,
                            RoleID = usuario.RoleID,
                            Activo = !usuario.Activo
                        };
                        var json = Newtonsoft.Json.JsonConvert.SerializeObject(dto);
                        var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
                        var res = await client.PutAsync($"api/usuarios/{usuario.UsuarioID}", content);
                        if (!res.IsSuccessStatusCode)
                        {
                            MessageBox.Show("Error cambiando estado: " + await res.Content.ReadAsStringAsync());
                            return;
                        }
                    }
                    dialog.Close();
                    await CargarUsuariosPorFiltro();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cambiar estado: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
            dialog.ShowDialog();
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
