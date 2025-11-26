using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfApp1.Services;

namespace WpfApp1.Views
{
    public partial class UsuariosWindow : Window
    {
        private readonly IPersonaService _personaService = new PersonaService();
        private ObservableCollection<UsuarioViewModel> _usuarios;
        private ObservableCollection<UsuarioViewModel> _usuariosFiltrados;
        private string _filtroEstado = "Activos";
        public bool IsAdmin { get; set; }

        public UsuariosWindow(bool isAdmin = false)
        {
            InitializeComponent();
            IsAdmin = isAdmin;
            DataContext = this;
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
            // Obtener el usuario actual y su rol
            int roleId = 1; // Por defecto Usuario
            if (Application.Current.Windows.OfType<HomeWindow>().FirstOrDefault() is HomeWindow home)
            {
                roleId = home.RoleID;
            }
            if (!CrearEditarUsuarioWindow.PuedeCrearUsuario(roleId))
            {
                MessageBox.Show("Solo los administradores pueden crear usuarios.", "Acceso Denegado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
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
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };
            stackPanel.Children.Add(new TextBlock { Text = "Información del Usuario", FontSize = 20, FontWeight = FontWeights.Bold, Foreground = (Brush)new BrushConverter().ConvertFromString("#2C3E50"), Margin = new Thickness(0, 0, 0, 18) });
            stackPanel.Children.Add(new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10), Children =
                {
                    new Border { Width = 48, Height = 48, CornerRadius = new CornerRadius(24), Background = (Brush)new BrushConverter().ConvertFromString("#2C3E50"), Child = new TextBlock { Text = usuario.InicialNombre, Foreground = Brushes.White, FontWeight = FontWeights.Bold, FontSize = 22, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center }, VerticalAlignment = VerticalAlignment.Center },
                    new StackPanel { Margin = new Thickness(12, 0, 0, 0), Children =
                        {
                            new TextBlock { Text = usuario.NombreCompleto, FontWeight = FontWeights.Bold, FontSize = 15, Foreground = (Brush)new BrushConverter().ConvertFromString("#2C3E50") },
                            new TextBlock { Text = usuario.Email, FontSize = 12, Foreground = (Brush)new BrushConverter().ConvertFromString("#7F8C8D"), Margin = new Thickness(0, 2, 0, 0) }
                        }
                    }
                }
            });
            stackPanel.Children.Add(new TextBlock { Text = $"Rol: {usuario.RolNombre}", FontSize = 14, Foreground = (Brush)new BrushConverter().ConvertFromString("#2C3E50"), Margin = new Thickness(0, 8, 0, 0) });
            stackPanel.Children.Add(new TextBlock { Text = $"Estado: {estado}", FontSize = 14, Foreground = (Brush)new BrushConverter().ConvertFromString(colorEstado), Margin = new Thickness(0, 8, 0, 0) });
            stackPanel.Children.Add(new TextBlock { Text = $"ID: {usuario.UsuarioID}", FontSize = 13, Foreground = (Brush)new BrushConverter().ConvertFromString("#7F8C8D"), Margin = new Thickness(0, 8, 0, 0) });
            var actionsPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 30, 0, 0), HorizontalAlignment = HorizontalAlignment.Right };
            var btnToggleUser = new Button { Content = usuario.Activo ? "Desactivar" : "Activar", Width = 120, Height = 38, Margin = new Thickness(0, 0, 12, 0), Background = usuario.Activo ? (Brush)new BrushConverter().ConvertFromString("#E74C3C") : (Brush)new BrushConverter().ConvertFromString("#27AE60"), Foreground = Brushes.White, FontWeight = FontWeights.Bold, BorderThickness = new Thickness(0), Cursor = System.Windows.Input.Cursors.Hand, Tag = usuario };
            actionsPanel.Children.Add(btnToggleUser);
            var btnClose = new Button { Content = "Cerrar", Width = 120, Height = 38, Background = (Brush)new BrushConverter().ConvertFromString("#2C3E50"), Foreground = Brushes.White, FontWeight = FontWeights.Bold, BorderThickness = new Thickness(0), Cursor = System.Windows.Input.Cursors.Hand, IsCancel = true };
            actionsPanel.Children.Add(btnClose);
            // Botón para modificar contraseña solo si eres admin
            if (IsAdmin)
            {
                var btnPassword = new Button { Content = "Modificar contraseña", Width = 180, Height = 38, Background = (Brush)new BrushConverter().ConvertFromString("#3498DB"), Foreground = Brushes.White, FontWeight = FontWeights.Bold, BorderThickness = new Thickness(0), Cursor = System.Windows.Input.Cursors.Hand, Margin = new Thickness(0, 0, 12, 0) };
                btnPassword.Click += async (s, ev) =>
                {
                    var inputDialog = new Window
                    {
                        Title = "Nueva contraseña",
                        Width = 600, // Mucho más ancho
                        Height = 320, // Mucho más alto
                        WindowStartupLocation = WindowStartupLocation.CenterOwner,
                        WindowStyle = WindowStyle.None,
                        ResizeMode = ResizeMode.NoResize,
                        Background = Brushes.White,
                        Owner = this,
                    };
                    var inputPanel = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(48) }; // Más margen
                    inputPanel.Children.Add(new TextBlock { Text = "Ingrese la nueva contraseña:", FontSize = 20, Margin = new Thickness(0, 0, 0, 24) });
                    var txtPassword = new PasswordBox { Height = 48, FontSize = 18, Margin = new Thickness(0, 0, 0, 32) };
                    inputPanel.Children.Add(txtPassword);
                    var btnAceptar = new Button { Content = "Aceptar", Width = 180, Height = 48, Background = (Brush)new BrushConverter().ConvertFromString("#27AE60"), Foreground = Brushes.White, FontWeight = FontWeights.Bold, BorderThickness = new Thickness(0), Cursor = System.Windows.Input.Cursors.Hand, Margin = new Thickness(0, 0, 24, 0) };
                    var btnCancelar = new Button { Content = "Cancelar", Width = 180, Height = 48, Background = (Brush)new BrushConverter().ConvertFromString("#E74C3C"), Foreground = Brushes.White, FontWeight = FontWeights.Bold, BorderThickness = new Thickness(0), Cursor = System.Windows.Input.Cursors.Hand };
                    var btnsPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
                    btnsPanel.Children.Add(btnAceptar);
                    btnsPanel.Children.Add(btnCancelar);
                    inputPanel.Children.Add(btnsPanel);
                    inputDialog.Content = inputPanel;
                    btnCancelar.Click += (s2, e2) => inputDialog.Close();
                    btnAceptar.Click += async (s2, e2) =>
                    {
                        var nuevaPassword = txtPassword.Password;
                        if (string.IsNullOrWhiteSpace(nuevaPassword))
                        {
                            MessageBox.Show("La contraseña no puede estar vacía.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        try
                        {
                            using (var client = new System.Net.Http.HttpClient())
                            {
                                client.BaseAddress = new Uri("http://localhost:5200/");
                                var json = Newtonsoft.Json.JsonConvert.SerializeObject(nuevaPassword);
                                var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
                                var request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod("PATCH"), $"api/Usuarios/{usuario.UsuarioID}/password")
                                {
                                    Content = content
                                };
                                var res = await client.SendAsync(request);
                                if (!res.IsSuccessStatusCode)
                                {
                                    MessageBox.Show("Error modificando contraseña: " + await res.Content.ReadAsStringAsync());
                                    return;
                                }
                                MessageBox.Show("Contraseña modificada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                                inputDialog.Close();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error al modificar contraseña: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    };
                    inputDialog.ShowDialog();
                };
                actionsPanel.Children.Insert(0, btnPassword);
            }
            stackPanel.Children.Add(actionsPanel);
            var dialog = new Window
            {
                Title = "Detalles del Usuario",
                Width = 700, // Mucho más ancho
                Height = 520, // Mucho más alto
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                Background = Brushes.White,
                Owner = this,
                Content = new Border
                {
                    Background = Brushes.White,
                    CornerRadius = new CornerRadius(16),
                    Padding = new Thickness(48), // Más margen
                    Effect = new System.Windows.Media.Effects.DropShadowEffect { Color = Colors.Black, BlurRadius = 30, Opacity = 0.13, ShadowDepth = 0 },
                    Child = stackPanel
                }
            };
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
