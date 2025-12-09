using System;
using System.Windows;
using WpfApp1.DTOs;
using WpfApp1.Services;

namespace WpfApp1.Views
{
    public partial class CrearEditarSIMWindow : Window
    {
        private readonly ISIMService _simService;
        private int? _simId;
        private bool _esEdicion;

        public event Action SIMGuardada;

        private SIMDTO _simActual;

        public CrearEditarSIMWindow()
        {
            InitializeComponent();
            _simService = new SIMService();
            _esEdicion = false;
            
            btnCancelar.Click += BtnCancelar_Click;
            btnGuardar.Click += BtnGuardar_Click;
        }

        public CrearEditarSIMWindow(int simId) : this()
        {
            _simId = simId;
            _esEdicion = true;
            txtTitulo.Text = "Editar Tarjeta SIM";
            this.Loaded += CrearEditarSIMWindow_Loaded;
            btnDesasignar.Click += BtnDesasignar_Click;
        }

        private async void CrearEditarSIMWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_esEdicion && _simId.HasValue)
            {
                try
                {
                    var sim = await _simService.GetByIdAsync(_simId.Value);
                    _simActual = sim;
                    if (sim != null)
                    {
                        txtNumeroSIM.Text = sim.NumeroSIM;
                        
                        btnDesasignar.Visibility = (sim.ProductoID.HasValue && sim.ProductoID.Value > 0)
                            ? Visibility.Visible
                            : Visibility.Collapsed;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar la SIM: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                }
            }
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarFormulario())
                return;

            try
            {
                btnGuardar.IsEnabled = false;

                var sim = new SIMDTO
                {
                    NumeroSIM = txtNumeroSIM.Text.Trim(),
                    ProductoID = null
                };

                bool resultado;

                if (_esEdicion && _simId.HasValue)
                {
                    sim.SIMID = _simId.Value;
                    resultado = await _simService.UpdateAsync(_simId.Value, sim);
                }
                else
                {
                    var simCreada = await _simService.CreateAsync(sim);
                    resultado = simCreada != null;
                }

                if (resultado)
                {
                    MessageBox.Show(
                        _esEdicion ? "SIM actualizada correctamente" : "SIM creada correctamente", 
                        "Exito", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                    
                    if (SIMGuardada != null)
                        SIMGuardada();
                    
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("No se pudo guardar la SIM", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    btnGuardar.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar la SIM: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                btnGuardar.IsEnabled = true;
            }
        }

        private async void BtnDesasignar_Click(object sender, RoutedEventArgs e)
        {
            if (_simActual == null || !_simActual.ProductoID.HasValue || _simActual.ProductoID.Value == 0)
            {
                MessageBox.Show("La SIM no está asignada a ningún producto.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var confirm = MessageBox.Show($"¿Seguro que deseas desasignar la SIM '{_simActual.NumeroSIM}' del producto?", "Confirmar desasignación", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes)
                return;
            try
            {
                var (exito, error) = await _simService.DesasignarProductoAsync(_simActual.SIMID);
                if (exito)
                {
                    MessageBox.Show("SIM desasignada correctamente. Ahora está disponible para asignar a otro producto.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    SIMGuardada?.Invoke();
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show($"No se pudo desasignar la SIM: {error}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al desasignar la SIM: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidarFormulario()
        {
            if (string.IsNullOrWhiteSpace(txtNumeroSIM.Text))
            {
                MessageBox.Show("El numero de SIM es obligatorio", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNumeroSIM.Focus();
                return false;
            }

            return true;
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
