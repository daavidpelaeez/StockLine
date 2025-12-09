using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using WpfApp1.DTOs;
using WpfApp1.Services;
using WpfApp1.ViewModels;
using System.Collections.Generic; 

namespace WpfApp1.Views
{
   
    public partial class AyuntamientosWindow : Window
    {
        private readonly IAyuntamientoService _ayuntamientoService;
        private ObservableCollection<AyuntamientoViewModel> _ayuntamientos;
        private ObservableCollection<AyuntamientoViewModel> _ayuntamientosFiltrados;

        private string _filtroEstado = "Todos"; 
        private string _textoBusqueda = "";

        public AyuntamientosWindow()
        {
            InitializeComponent();

            _ayuntamientoService = new AyuntamientoService();
            _ayuntamientos = new ObservableCollection<AyuntamientoViewModel>();
            _ayuntamientosFiltrados = new ObservableCollection<AyuntamientoViewModel>();

            dgAyuntamientos.ItemsSource = _ayuntamientosFiltrados;
            txtBuscar.TextChanged += TxtBuscar_TextChanged;
            this.Loaded += AyuntamientosWindow_Loaded;
        }

        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            _textoBusqueda = txtBuscar.Text.Trim();
            AplicarFiltros();
        }

        private async void AyuntamientosWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (dgAyuntamientos != null)
            {
                dgAyuntamientos.ItemsSource = _ayuntamientosFiltrados;
                await CargarAyuntamientos();
            }
            
        }

        private async System.Threading.Tasks.Task CargarAyuntamientos()
        {
            try
            {
                this.Cursor = System.Windows.Input.Cursors.Wait;
                if (dgAyuntamientos == null)
                {
                   
                    return;
                }
                
                string query = null;
                if (_filtroEstado == "Activos") query = "?activos=true";
                else if (_filtroEstado == "Inactivos") query = "?activos=false";

                List<AyuntamientoDTO> ayuntamientos = null;
                try
                {
                    ayuntamientos = await _ayuntamientoService.GetAllAsync(query);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al obtener ayuntamientos: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                if (ayuntamientos == null)
                    ayuntamientos = new List<AyuntamientoDTO>();

                
                var validos = ayuntamientos.Where(a => a != null).ToList();

                _ayuntamientos.Clear();
                foreach (var ayuntamiento in validos)
                {
                    _ayuntamientos.Add(new AyuntamientoViewModel
                    {
                        AyuntamientoID = ayuntamiento.AyuntamientoID,
                        Nombre = ayuntamiento.Nombre ?? "Sin nombre",
                        Direccion = ayuntamiento.Direccion ?? "Sin dirección",
                        CP = ayuntamiento.CP ?? "",
                        Ciudad = ayuntamiento.Ciudad ?? "",
                        Provincia = ayuntamiento.Provincia ?? "",
                        Telefono = ayuntamiento.Telefono ?? "Sin teléfono",
                        Email = ayuntamiento.Email ?? "Sin email",
                        ComercialID = ayuntamiento.ComercialID,
                        ComercialNombre = ayuntamiento.ComercialNombre ?? "Sin asignar",
                        InicialNombre = string.IsNullOrEmpty(ayuntamiento.Nombre) ? "?" : ayuntamiento.Nombre.Substring(0, 1).ToUpper(),
                        Activo = ayuntamiento.Activo
                    });
                }
                AplicarFiltros();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error inesperado al cargar ayuntamientos: " + ex.Message, "Error Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
                _ayuntamientos.Clear();
                _ayuntamientosFiltrados.Clear();
            }
            finally
            {
                this.Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        private void AplicarFiltros()
        {
            try
            {
                var resultado = _ayuntamientos.AsEnumerable();
                if (!string.IsNullOrWhiteSpace(_textoBusqueda))
                {
                    string filtro = _textoBusqueda.ToLower();
                    resultado = resultado.Where(a =>
                        (a.Nombre != null && a.Nombre.ToLower().Contains(filtro)) ||
                        (a.Direccion != null && a.Direccion.ToLower().Contains(filtro)) ||
                        (a.Email != null && a.Email.ToLower().Contains(filtro)) ||
                        (a.Telefono != null && a.Telefono.ToLower().Contains(filtro))
                    );
                }
                if (_filtroEstado != "Todos")
                {
                    bool estadoActivo = _filtroEstado == "Activos";
                    resultado = resultado.Where(a => a.Activo == estadoActivo);
                }
                _ayuntamientosFiltrados.Clear();
                foreach (var ayuntamiento in resultado)
                {
                    _ayuntamientosFiltrados.Add(ayuntamiento);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en AplicarFiltros: " + ex.Message);
                _ayuntamientosFiltrados.Clear();
                foreach (var ayuntamiento in _ayuntamientos)
                {
                    _ayuntamientosFiltrados.Add(ayuntamiento);
                }
            }
        }

        private void BtnNuevoAyuntamiento_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new AddEditAyuntamientos();
            ventana.AyuntamientoGuardado += async () => await CargarAyuntamientos();
            ventana.ShowDialog();
        }

        private void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            CargarAyuntamientos();
        }

        private async void BtnVer_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var ayuntamiento = button?.Tag as AyuntamientoViewModel;

            if (ayuntamiento == null)
            {
                MessageBox.Show("Selecciona un ayuntamiento", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var estado = ayuntamiento.Activo ? "Activo" : "Inactivo";
            var colorEstado = ayuntamiento.Activo ? "#27AE60" : "#E74C3C";
            var dialog = new Window
            {
                Title = "Detalles del Ayuntamiento",
                Width = 480,
                Height = 540,
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
                            new TextBlock { Text = "Información del Ayuntamiento", FontSize = 20, FontWeight = FontWeights.Bold, Foreground = (Brush)new BrushConverter().ConvertFromString("#2C3E50"), Margin = new Thickness(0,0,0,18) },
                            new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0,0,0,10), Children =
                                {
                                    new Border { Width = 48, Height = 48, CornerRadius = new CornerRadius(24), Background = (Brush)new BrushConverter().ConvertFromString("#2C3E50"), Child = new TextBlock { Text = ayuntamiento.InicialNombre, Foreground = Brushes.White, FontWeight = FontWeights.Bold, FontSize = 22, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center }, VerticalAlignment = VerticalAlignment.Center },
                                    new StackPanel { Margin = new Thickness(12,0,0,0), Children =
                                        {
                                            new TextBlock { Text = ayuntamiento.Nombre, FontWeight = FontWeights.Bold, FontSize = 15, Foreground = (Brush)new BrushConverter().ConvertFromString("#2C3E50") },
                                            new TextBlock { Text = ayuntamiento.Email, FontSize = 12, Foreground = (Brush)new BrushConverter().ConvertFromString("#7F8C8D"), Margin = new Thickness(0,2,0,0) }
                                        }
                                    }
                                }
                            },
                            new TextBlock { Text = $"Dirección: {ayuntamiento.Direccion}", FontSize = 14, Foreground = (Brush)new BrushConverter().ConvertFromString("#7F8C8D"), Margin = new Thickness(0,8,0,0) },
                            new TextBlock { Text = $"Ciudad: {ayuntamiento.Ciudad}", FontSize = 14, Foreground = (Brush)new BrushConverter().ConvertFromString("#7F8C8D"), Margin = new Thickness(0,4,0,0) },
                            new TextBlock { Text = $"Provincia: {ayuntamiento.Provincia}", FontSize = 14, Foreground = (Brush)new BrushConverter().ConvertFromString("#7F8C8D"), Margin = new Thickness(0,4,0,0) },
                            new TextBlock { Text = $"Código Postal: {ayuntamiento.CP}", FontSize = 14, Foreground = (Brush)new BrushConverter().ConvertFromString("#7F8C8D"), Margin = new Thickness(0,4,0,0) },
                            new TextBlock { Text = $"Teléfono: {ayuntamiento.Telefono}", FontSize = 14, Foreground = (Brush)new BrushConverter().ConvertFromString("#7F8C8D"), Margin = new Thickness(0,4,0,0) },
                            new TextBlock { Text = $"Comercial: {ayuntamiento.ComercialNombre}", FontSize = 14, Foreground = (Brush)new BrushConverter().ConvertFromString("#2ECC71"), Margin = new Thickness(0,8,0,0) },
                            new TextBlock { Text = $"Estado: {estado}", FontSize = 14, Foreground = (Brush)new BrushConverter().ConvertFromString(colorEstado), Margin = new Thickness(0,8,0,0) },
                            new TextBlock { Text = $"ID: {ayuntamiento.AyuntamientoID}", FontSize = 13, Foreground = (Brush)new BrushConverter().ConvertFromString("#7F8C8D"), Margin = new Thickness(0,8,0,0) },
                            new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0,30,0,0), HorizontalAlignment = HorizontalAlignment.Right, Children =
                                {
                                    new Button { Content = ayuntamiento.Activo ? "Desactivar" : "Activar", Width = 120, Height = 38, Margin = new Thickness(0,0,12,0), Background = ayuntamiento.Activo ? (Brush)new BrushConverter().ConvertFromString("#E74C3C") : (Brush)new BrushConverter().ConvertFromString("#27AE60"), Foreground = Brushes.White, FontWeight = FontWeights.Bold, BorderThickness = new Thickness(0), Cursor = System.Windows.Input.Cursors.Hand, Tag = ayuntamiento },
                                    new Button { Content = "Cerrar", Width = 120, Height = 38, Background = (Brush)new BrushConverter().ConvertFromString("#2C3E50"), Foreground = Brushes.White, FontWeight = FontWeights.Bold, BorderThickness = new Thickness(0), Cursor = System.Windows.Input.Cursors.Hand, IsCancel = true }
                                }
                            }
                        }
                    }
                }
            };
            
            StackPanel mainPanel = (dialog.Content as Border)?.Child as StackPanel;
            Button btnToggleButton = null;
            if (mainPanel != null)
            {
                foreach (var child in mainPanel.Children)
                {
                    if (child is StackPanel sp && sp.Orientation == Orientation.Horizontal && sp.HorizontalAlignment == HorizontalAlignment.Right)
                    {
                        foreach (var btnObj in sp.Children)
                        {
                            if (btnObj is Button btn && (btn.Content?.ToString() == "Desactivar" || btn.Content?.ToString() == "Activar"))
                            {
                                btnToggleButton = btn;
                                break;
                            }
                        }
                    }
                    if (btnToggleButton != null) break;
                }
            }
            if (btnToggleButton == null)
            {
                MessageBox.Show("No se encontró el botón de activar/desactivar.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                dialog.ShowDialog();
                return;
            }
            btnToggleButton.Click += async (s, ev) =>
            {
                try
                {
                    var dto = new AyuntamientoDTO
                    {
                        AyuntamientoID = ayuntamiento.AyuntamientoID,
                        Nombre = ayuntamiento.Nombre,
                        Direccion = ayuntamiento.Direccion,
                        CP = ayuntamiento.CP,
                        Ciudad = ayuntamiento.Ciudad,
                        Provincia = ayuntamiento.Provincia,
                        Telefono = ayuntamiento.Telefono,
                        Email = ayuntamiento.Email,
                        ComercialID = ayuntamiento.ComercialID,
                        Activo = !ayuntamiento.Activo 
                    };

                   
                    var respuesta = await _ayuntamientoService.UpdateAsync(dto);

                    if (respuesta)
                    {
                        dialog.Close();
                        MessageBox.Show("Estado del ayuntamiento actualizado con éxito.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                        await CargarAyuntamientos(); 
                    }
                    else
                    {
                        MessageBox.Show("Error al actualizar el ayuntamiento. Inténtalo de nuevo.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Error inesperado:\n\n" + ex.Message,
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            };

            dialog.ShowDialog();
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

        private void ComboEstado_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = sender as ComboBox;
            if (combo?.SelectedItem is ComboBoxItem item)
            {
                _filtroEstado = item.Content.ToString();
                CargarAyuntamientos();
            }
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var ayuntamiento = button?.Tag as AyuntamientoViewModel;
            if (ayuntamiento == null)
            {
                MessageBox.Show("Selecciona un ayuntamiento para editar.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var ventana = new AddEditAyuntamientos(ayuntamiento); 
            ventana.AyuntamientoGuardado += async () => await CargarAyuntamientos();
            ventana.ShowDialog();
        }

        private async void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var ayuntamiento = button?.Tag as AyuntamientoViewModel;
            if (ayuntamiento == null)
            {
                MessageBox.Show("Selecciona un ayuntamiento para eliminar.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var confirm = MessageBox.Show($"¿Seguro que deseas eliminar el ayuntamiento '{ayuntamiento.Nombre}'?", "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm == MessageBoxResult.Yes)
            {
                bool eliminado = false;
                try
                {
                    eliminado = await _ayuntamientoService.DeleteAsync(ayuntamiento.AyuntamientoID);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al eliminar: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                if (eliminado)
                {
                    MessageBox.Show("Ayuntamiento eliminado correctamente.", "Eliminado", MessageBoxButton.OK, MessageBoxImage.Information);
                    await CargarAyuntamientos();
                }
                else
                {
                    MessageBox.Show("No se pudo eliminar el ayuntamiento.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
