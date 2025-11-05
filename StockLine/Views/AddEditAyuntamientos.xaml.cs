using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using WpfApp1.DTOs;
using WpfApp1.Services;

namespace WpfApp1.Views
{
    /// <summary>
    /// Interaction logic for AddEditAyuntamientos.xaml
    /// </summary>
    public partial class AddEditAyuntamientos : Window
    {
        private readonly IAyuntamientoService _ayuntamientoService;
        private readonly IComercialService _comercialService;
        private AyuntamientoViewModel _ayuntamientoEdicion;
        private bool _esEdicion;
        private List<ComercialViewModel> _comerciales;

        public event Action AyuntamientoGuardado;

        public AddEditAyuntamientos()
        {
            InitializeComponent();
            _ayuntamientoService = new AyuntamientoService();
            _comercialService = new ComercialService();
            _esEdicion = false;
            _comerciales = new List<ComercialViewModel>();

            this.Loaded += AddEditAyuntamientos_Loaded;
        }

        public AddEditAyuntamientos(AyuntamientoViewModel ayuntamiento) : this()
        {
            _ayuntamientoEdicion = ayuntamiento;
            _esEdicion = true;

            txtTitulo.Text = "Editar Ayuntamiento";
            this.Title = "Editar Ayuntamiento";
        }

        private async void AddEditAyuntamientos_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarComerciales();

            if (_esEdicion)
            {
                CargarDatosAyuntamiento();
            }
        }

        private async System.Threading.Tasks.Task CargarComerciales()
        {
            try
            {
                this.Cursor = System.Windows.Input.Cursors.Wait;

                var comerciales = await _comercialService.GetAllAsync();

                if (comerciales == null)
                {
                    comerciales = new List<ComercialDTO>();
                }

                _comerciales.Clear();

                // Agregar opción "Sin asignar"
                _comerciales.Add(new ComercialViewModel
                {
                    ComercialID = 0,
                    NombreCompleto = "Sin asignar",
                    Email = ""
                });

                // Agregar comerciales de la base de datos
                foreach (var comercial in comerciales)
                {
                    _comerciales.Add(new ComercialViewModel
                    {
                        ComercialID = comercial.ComercialID,
                        Nombre = comercial.Nombre,
                        Apellidos = comercial.Apellidos,
                        NombreCompleto = comercial.Nombre + " " + comercial.Apellidos,
                        Email = comercial.Email ?? "",
                        Telefono = comercial.Telefono ?? ""
                    });
                }

                cbComercial.ItemsSource = _comerciales;
                cbComercial.SelectedIndex = 0; // Seleccionar "Sin asignar" por defecto
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar comerciales:\n\n" + ex.Message + "\n\n" +
                    "Podrás guardar el ayuntamiento sin comercial asignado.",
                    "Advertencia",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                // Asegurar que al menos tenga la opción "Sin asignar"
                _comerciales.Clear();
                _comerciales.Add(new ComercialViewModel
                {
                    ComercialID = 0,
                    NombreCompleto = "Sin asignar",
                    Email = ""
                });
                cbComercial.ItemsSource = _comerciales;
                cbComercial.SelectedIndex = 0;
            }
            finally
            {
                this.Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        private void CargarDatosAyuntamiento()
        {
            if (_ayuntamientoEdicion == null) return;

            try
            {
                txtNombre.Text = _ayuntamientoEdicion.Nombre;
                txtDireccion.Text = _ayuntamientoEdicion.Direccion;
                txtCP.Text = _ayuntamientoEdicion.CP;
                txtCiudad.Text = _ayuntamientoEdicion.Ciudad;
                txtProvincia.Text = _ayuntamientoEdicion.Provincia;
                txtTelefono.Text = _ayuntamientoEdicion.Telefono;
                txtEmail.Text = _ayuntamientoEdicion.Email;

                // Seleccionar el comercial asignado
                if (_ayuntamientoEdicion.ComercialID.HasValue && _ayuntamientoEdicion.ComercialID.Value > 0)
                {
                    var comercialSeleccionado = _comerciales.FirstOrDefault(c => c.ComercialID == _ayuntamientoEdicion.ComercialID.Value);
                    if (comercialSeleccionado != null)
                    {
                        cbComercial.SelectedItem = comercialSeleccionado;
                    }
                    else
                    {
                        // Si no se encuentra el comercial, seleccionar "Sin asignar"
                        cbComercial.SelectedIndex = 0;
                    }
                }
                else
                {
                    // Sin comercial asignado
                    cbComercial.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error al cargar datos del ayuntamiento:\n\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarFormulario())
                return;

            try
            {
                btnGuardar.IsEnabled = false;
                btnCancelar.IsEnabled = false;

                var comercialSeleccionado = cbComercial.SelectedItem as ComercialViewModel;
                int? comercialID = null;

                if (comercialSeleccionado != null && comercialSeleccionado.ComercialID > 0)
                {
                    comercialID = comercialSeleccionado.ComercialID;
                }

                // Log para depuración
                System.Diagnostics.Debug.WriteLine($"=== GUARDANDO AYUNTAMIENTO ===");
                System.Diagnostics.Debug.WriteLine($"Nombre: {txtNombre.Text.Trim()}");
                System.Diagnostics.Debug.WriteLine($"Dirección: {txtDireccion.Text.Trim()}");
                System.Diagnostics.Debug.WriteLine($"CP: {txtCP.Text.Trim()}");
                System.Diagnostics.Debug.WriteLine($"Ciudad: {txtCiudad.Text.Trim()}");
                System.Diagnostics.Debug.WriteLine($"Provincia: {txtProvincia.Text.Trim()}");
                System.Diagnostics.Debug.WriteLine($"Teléfono: {txtTelefono.Text.Trim()}");
                System.Diagnostics.Debug.WriteLine($"Email: {txtEmail.Text.Trim()}");
                System.Diagnostics.Debug.WriteLine($"Comercial ID: {comercialID}");
                System.Diagnostics.Debug.WriteLine($"Comercial Seleccionado: {comercialSeleccionado?.NombreCompleto}");

                var ayuntamiento = new AyuntamientoDTO
                {
                    Nombre = txtNombre.Text.Trim(),
                    Direccion = txtDireccion.Text.Trim(),
                    CP = txtCP.Text.Trim(),
                    Ciudad = txtCiudad.Text.Trim(),
                    Provincia = txtProvincia.Text.Trim(),
                    Telefono = txtTelefono.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    ComercialID = comercialID
                };

                // Log del objeto antes de enviarlo
                var jsonDebug = Newtonsoft.Json.JsonConvert.SerializeObject(ayuntamiento, Newtonsoft.Json.Formatting.Indented);
                System.Diagnostics.Debug.WriteLine($"JSON a enviar:\n{jsonDebug}");

                bool resultado;

                if (_esEdicion)
                {
                    ayuntamiento.AyuntamientoID = _ayuntamientoEdicion.AyuntamientoID;
                    resultado = await _ayuntamientoService.UpdateAsync(ayuntamiento);
                }
                else
                {
                    resultado = await _ayuntamientoService.CreateAsync(ayuntamiento);
                }

                if (resultado)
                {
                    MessageBox.Show(
                        _esEdicion ? "Ayuntamiento actualizado correctamente" : "Ayuntamiento creado correctamente",
                        "Éxito",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    if (AyuntamientoGuardado != null)
                        AyuntamientoGuardado();

                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("No se pudo guardar el ayuntamiento", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    btnGuardar.IsEnabled = true;
                    btnCancelar.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR al guardar: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                MessageBox.Show("Error al guardar el ayuntamiento: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                btnGuardar.IsEnabled = true;
                btnCancelar.IsEnabled = true;
            }
        }

        private bool ValidarFormulario()
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre es obligatorio", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNombre.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtDireccion.Text))
            {
                MessageBox.Show("La dirección es obligatoria", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtDireccion.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtCP.Text))
            {
                MessageBox.Show("El código postal es obligatorio", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCP.Focus();
                return false;
            }

            if (txtCP.Text.Length != 5 || !txtCP.Text.All(char.IsDigit))
            {
                MessageBox.Show("El código postal debe tener 5 dígitos", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCP.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtCiudad.Text))
            {
                MessageBox.Show("La ciudad es obligatoria", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtCiudad.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtProvincia.Text))
            {
                MessageBox.Show("La provincia es obligatoria", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtProvincia.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtTelefono.Text))
            {
                MessageBox.Show("El teléfono es obligatorio", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTelefono.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                MessageBox.Show("El email es obligatorio", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEmail.Focus();
                return false;
            }

            if (!txtEmail.Text.Contains("@") || !txtEmail.Text.Contains("."))
            {
                MessageBox.Show("El email no tiene un formato válido", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEmail.Focus();
                return false;
            }

            if (cbComercial.SelectedItem == null)
            {
                MessageBox.Show("Debes seleccionar un comercial (o 'Sin asignar')", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                cbComercial.Focus();
                return false;
            }

            return true;
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        // Métodos vacíos que ya no se usan pero los mantenemos para evitar errores
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
        }
    }

    // ViewModel para Comercial en el ComboBox
    public class ComercialViewModel
    {
        public int ComercialID { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string NombreCompleto { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
    }
}
