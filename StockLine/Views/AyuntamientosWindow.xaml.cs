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
    /// Interaction logic for AyuntamientosWindow.xaml
    /// </summary>
    public partial class AyuntamientosWindow : Window
    {
        private readonly IAyuntamientoService _ayuntamientoService;
        private ObservableCollection<AyuntamientoViewModel> _ayuntamientos;
        private ObservableCollection<AyuntamientoViewModel> _ayuntamientosFiltrados;

        public AyuntamientosWindow()
        {
            InitializeComponent();

            _ayuntamientoService = new AyuntamientoService();
            _ayuntamientos = new ObservableCollection<AyuntamientoViewModel>();
            _ayuntamientosFiltrados = new ObservableCollection<AyuntamientoViewModel>();

            dgAyuntamientos.ItemsSource = _ayuntamientosFiltrados;

            this.Loaded += AyuntamientosWindow_Loaded;
        }

        private async void AyuntamientosWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarAyuntamientos();
        }

        private async System.Threading.Tasks.Task CargarAyuntamientos()
        {
            try
            {
                // Mostrar cursor de espera
                this.Cursor = System.Windows.Input.Cursors.Wait;
                
                var ayuntamientos = await _ayuntamientoService.GetAllAsync();
                
                // Verificar que la lista no sea nula
                if (ayuntamientos == null)
                {
                    ayuntamientos = new System.Collections.Generic.List<AyuntamientoDTO>();
                }
                
                _ayuntamientos.Clear();

                foreach (var ayuntamiento in ayuntamientos)
                {
                    try
                    {
                        // Log para depuración
                        System.Diagnostics.Debug.WriteLine($"=== Cargando Ayuntamiento ID: {ayuntamiento.AyuntamientoID} ===");
                        System.Diagnostics.Debug.WriteLine($"Nombre: {ayuntamiento.Nombre}");
                        System.Diagnostics.Debug.WriteLine($"ComercialID: {ayuntamiento.ComercialID}");
                        System.Diagnostics.Debug.WriteLine($"ComercialNombre: {ayuntamiento.ComercialNombre}");
                        
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
                            InicialNombre = string.IsNullOrEmpty(ayuntamiento.Nombre) ? "?" : ayuntamiento.Nombre.Substring(0, 1).ToUpper()
                        });
                    }
                    catch (Exception exItem)
                    {
                        // Si un item individual falla, continuar con los demás
                        System.Diagnostics.Debug.WriteLine("Error al procesar ayuntamiento ID " + ayuntamiento.AyuntamientoID + ": " + exItem.Message);
                    }
                }

                AplicarFiltros();
                ActualizarEstadisticas();
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("conexión") || ex.Message.Contains("API"))
                {
                    MessageBox.Show(
                        "Error de conexión con el servidor.\n\n" +
                        "Verifica que la API esté ejecutándose.\n\n" +
                        "Detalles: " + ex.Message,
                        "Error de Conexión",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show(
                        "Error al cargar ayuntamientos:\n\n" + ex.Message,
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
                
                // Asegurar que las listas estén inicializadas aunque falle
                _ayuntamientos.Clear();
                _ayuntamientosFiltrados.Clear();
                txtTotalAyuntamientos.Text = "Total de ayuntamientos: 0";
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error inesperado al cargar ayuntamientos:\n\n" +
                    "Tipo: " + ex.GetType().Name + "\n" +
                    "Mensaje: " + ex.Message,
                    "Error Crítico",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                
                // Asegurar que las listas estén inicializadas aunque falle
                _ayuntamientos.Clear();
                _ayuntamientosFiltrados.Clear();
                txtTotalAyuntamientos.Text = "Total de ayuntamientos: 0";
            }
            finally
            {
                // Restaurar cursor normal
                this.Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        private void AplicarFiltros()
        {
            try
            {
                var resultado = _ayuntamientos.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(txtBuscar.Text))
                {
                    var busqueda = txtBuscar.Text.ToLower();
                    resultado = resultado.Where(a =>
                        (a.Nombre != null && a.Nombre.ToLower().Contains(busqueda)) ||
                        (a.Direccion != null && a.Direccion.ToLower().Contains(busqueda)) ||
                        (a.Telefono != null && a.Telefono.ToLower().Contains(busqueda)) ||
                        (a.Email != null && a.Email.ToLower().Contains(busqueda)) ||
                        (a.ComercialNombre != null && a.ComercialNombre.ToLower().Contains(busqueda))
                    );
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
                
                // Si hay error, mostrar todos sin filtro
                _ayuntamientosFiltrados.Clear();
                foreach (var ayuntamiento in _ayuntamientos)
                {
                    _ayuntamientosFiltrados.Add(ayuntamiento);
                }
            }
        }

        private void ActualizarEstadisticas()
        {
            try
            {
                txtTotalAyuntamientos.Text = "Total de ayuntamientos: " + _ayuntamientosFiltrados.Count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ActualizarEstadisticas: " + ex.Message);
                txtTotalAyuntamientos.Text = "Total de ayuntamientos: 0";
            }
        }

        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            AplicarFiltros();
            ActualizarEstadisticas();
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

        private void BtnVer_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var ayuntamiento = button?.Tag as AyuntamientoViewModel;

            if (ayuntamiento == null)
            {
                MessageBox.Show("Selecciona un ayuntamiento", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var detalles = "Información del Ayuntamiento:\n\n" +
                          "ID: " + ayuntamiento.AyuntamientoID + "\n" +
                          "Nombre: " + ayuntamiento.Nombre + "\n" +
                          "Dirección: " + ayuntamiento.Direccion + "\n" +
                          "Código Postal: " + ayuntamiento.CP + "\n" +
                          "Ciudad: " + ayuntamiento.Ciudad + "\n" +
                          "Provincia: " + ayuntamiento.Provincia + "\n" +
                          "Teléfono: " + ayuntamiento.Telefono + "\n" +
                          "Email: " + ayuntamiento.Email + "\n" +
                          "Comercial: " + ayuntamiento.ComercialNombre;

            MessageBox.Show(detalles, "Detalles del Ayuntamiento", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var ayuntamiento = button?.Tag as AyuntamientoViewModel;

            if (ayuntamiento == null)
            {
                MessageBox.Show("Selecciona un ayuntamiento para editar", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var ventana = new AddEditAyuntamientos(ayuntamiento);
            ventana.AyuntamientoGuardado += async () => await CargarAyuntamientos();
            ventana.ShowDialog();
        }

        private async void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            // Deshabilitar el botón para evitar múltiples clicks
            var button = sender as Button;
            if (button == null) return;
            
            var ayuntamiento = button?.Tag as AyuntamientoViewModel;

            if (ayuntamiento == null)
            {
                MessageBox.Show("Selecciona un ayuntamiento para eliminar", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirmacion = MessageBox.Show(
                "¿Estás seguro de eliminar el ayuntamiento '" + ayuntamiento.Nombre + "'?\n\n" +
                "ADVERTENCIA: No se podrá eliminar si tiene registros asociados.\n\n" +
                "Esta acción NO se puede deshacer.",
                "Confirmar Eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmacion != MessageBoxResult.Yes)
                return;

            // Deshabilitar botón mientras se procesa
            var originalIsEnabled = button.IsEnabled;
            button.IsEnabled = false;

            try
            {
                var resultado = await _ayuntamientoService.DeleteAsync(ayuntamiento.AyuntamientoID);

                if (resultado)
                {
                    MessageBox.Show(
                        "Ayuntamiento eliminado correctamente", 
                        "Éxito", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                    await CargarAyuntamientos();
                }
                else
                {
                    MessageBox.Show(
                        "No se pudo eliminar el ayuntamiento.\n" +
                        "Inténtalo nuevamente o contacta con el administrador.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (ArgumentException argEx)
            {
                MessageBox.Show(
                    "Datos inválidos:\n\n" + argEx.Message,
                    "Error de Validación",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (InvalidOperationException ex)
            {
                // Detectar si es un error de registros asociados
                if (ex.Message.Contains("registros asociados") || 
                    ex.Message.Contains("REFERENCE constraint") ||
                    ex.Message.Contains("envíos") ||
                    ex.Message.Contains("comerciales") ||
                    ex.Message.Contains("Conflict"))
                {
                    MessageBox.Show(
                        "⚠️ NO SE PUEDE ELIMINAR ESTE AYUNTAMIENTO\n\n" +
                        "El ayuntamiento tiene registros asociados.\n\n" +
                        "Posibles causas:\n" +
                        "• Tiene envíos registrados\n" +
                        "• Tiene comerciales asignados\n" +
                        "• Tiene otros registros dependientes\n\n" +
                        "SOLUCIONES:\n" +
                        "1. Reasignar los registros a otro ayuntamiento\n" +
                        "2. Eliminar primero los registros asociados\n" +
                        "3. Contactar con el administrador\n\n" +
                        "Detalles técnicos:\n" + ex.Message,
                        "Ayuntamiento con Registros Asociados",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
                else if (ex.Message.Contains("no existe") || 
                         ex.Message.Contains("ya fue eliminado") ||
                         ex.Message.Contains("NotFound"))
                {
                    MessageBox.Show(
                        "⚠️ AYUNTAMIENTO NO ENCONTRADO\n\n" +
                        "El ayuntamiento no existe o ya fue eliminado.\n\n" +
                        "Se actualizará la lista automáticamente.",
                        "Ayuntamiento No Encontrado",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    await CargarAyuntamientos();
                }
                else if (ex.Message.Contains("conexión") || 
                         ex.Message.Contains("servidor") ||
                         ex.Message.Contains("API"))
                {
                    MessageBox.Show(
                        "❌ ERROR DE CONEXIÓN\n\n" +
                        "No se pudo conectar con el servidor.\n\n" +
                        "VERIFICA:\n" +
                        "• Que la API esté ejecutándose\n" +
                        "• Que el servidor esté en http://localhost:5200/\n" +
                        "• Tu conexión a internet\n\n" +
                        "Detalles técnicos:\n" + ex.Message,
                        "Error de Conexión",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
                else if (ex.Message.Contains("permisos") || 
                         ex.Message.Contains("Unauthorized") ||
                         ex.Message.Contains("Forbidden"))
                {
                    MessageBox.Show(
                        "🔒 SIN PERMISOS\n\n" +
                        "No tienes permisos para eliminar este ayuntamiento.\n\n" +
                        "Contacta con el administrador del sistema.",
                        "Acceso Denegado",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
                else if (ex.Message.Contains("tiempo de espera") ||
                         ex.Message.Contains("timeout") ||
                         ex.Message.Contains("excedido"))
                {
                    MessageBox.Show(
                        "⏱️ TIEMPO DE ESPERA AGOTADO\n\n" +
                        "La operación tardó demasiado tiempo.\n\n" +
                        "POSIBLES CAUSAS:\n" +
                        "• El servidor está sobrecargado\n" +
                        "• Problemas de red\n" +
                        "• La base de datos está procesando otras operaciones\n\n" +
                        "Intenta nuevamente en unos momentos.",
                        "Timeout",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
                else
                {
                    // Error genérico pero controlado
                    MessageBox.Show(
                        "❌ ERROR AL ELIMINAR\n\n" +
                        ex.Message + "\n\n" +
                        "Si el problema persiste, contacta con el administrador.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                // Error completamente inesperado
                MessageBox.Show(
                    "💥 ERROR INESPERADO\n\n" +
                    "Ha ocurrido un error no controlado:\n\n" +
                    "Tipo: " + ex.GetType().Name + "\n" +
                    "Mensaje: " + ex.Message + "\n\n" +
                    "Por favor, informa al administrador del sistema.\n\n" +
                    "Stack Trace (para soporte técnico):\n" + ex.StackTrace,
                    "Error Crítico",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                // Re-habilitar el botón
                button.IsEnabled = originalIsEnabled;
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

        // Métodos vacíos que ya no se usan pero los mantenemos para evitar errores
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        private void GrdAyuntamientos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
        }
    }

    public class AyuntamientoViewModel
    {
        public int AyuntamientoID { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public string CP { get; set; }
        public string Ciudad { get; set; }
        public string Provincia { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public int? ComercialID { get; set; }
        public string ComercialNombre { get; set; }
        public string InicialNombre { get; set; }
    }
}
