using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.DTOs;
using WpfApp1.Services;

namespace WpfApp1.Views
{
    public partial class GestionSIMsWindow : Window
    {
        private readonly ISIMService _simService;
        private ObservableCollection<SIMViewModel> _sims;
        private ObservableCollection<SIMViewModel> _simsFiltradas;

        public GestionSIMsWindow()
        {
            InitializeComponent();
            
            _simService = new SIMService();
            _sims = new ObservableCollection<SIMViewModel>();
            _simsFiltradas = new ObservableCollection<SIMViewModel>();
            
            dgSIMs.ItemsSource = _simsFiltradas;
            
            this.Loaded += GestionSIMsWindow_Loaded;
            txtBuscar.TextChanged += TxtBuscar_TextChanged;
            cbFiltroEstado.SelectionChanged += CbFiltroEstado_SelectionChanged;
            btnNuevaSIM.Click += BtnNuevaSIM_Click;
            btnActualizar.Click += BtnActualizar_Click;
        }

        private async void GestionSIMsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarSIMs();
        }

        private async System.Threading.Tasks.Task CargarSIMs()
        {
            try
            {
                var sims = await _simService.GetAllAsync();
                _sims.Clear();

                foreach (var sim in sims)
                {
                    _sims.Add(new SIMViewModel
                    {
                        SIMID = sim.SIMID,
                        NumeroSIM = sim.NumeroSIM,
                        ProductoID = sim.ProductoID ?? 0,
                        ProductoNombre = string.IsNullOrEmpty(sim.ProductoNombre) ? "Sin asignar" : sim.ProductoNombre,
                        Estado = (sim.ProductoID == null || sim.ProductoID == 0) ? "Disponible" : "Asignada"
                    });
                }

                AplicarFiltros();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar SIMs: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AplicarFiltros()
        {
            var resultado = _sims.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(txtBuscar.Text))
            {
                var busqueda = txtBuscar.Text.ToLower();
                resultado = resultado.Where(s => 
                    s.NumeroSIM.ToLower().Contains(busqueda) ||
                    s.ProductoNombre.ToLower().Contains(busqueda)
                );
            }

            if (cbFiltroEstado.SelectedIndex == 1)
            {
                resultado = resultado.Where(s => s.Estado == "Disponible");
            }
            else if (cbFiltroEstado.SelectedIndex == 2)
            {
                resultado = resultado.Where(s => s.Estado == "Asignada");
            }

            _simsFiltradas.Clear();
            foreach (var sim in resultado)
            {
                _simsFiltradas.Add(sim);
            }

            txtTotalSIMs.Text = "Total de SIMs: " + _simsFiltradas.Count;
        }

        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            AplicarFiltros();
        }

        private void CbFiltroEstado_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AplicarFiltros();
        }

        private void BtnNuevaSIM_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new CrearEditarSIMWindow();
            ventana.SIMGuardada += async () => await CargarSIMs();
            ventana.ShowDialog();
        }

        private void BtnEditarSIM_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var sim = button?.Tag as SIMViewModel;

            if (sim == null)
            {
                MessageBox.Show("Selecciona una SIM para editar", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var ventana = new CrearEditarSIMWindow(sim.SIMID);
            ventana.SIMGuardada += async () => await CargarSIMs();
            ventana.ShowDialog();
        }

        private void BtnAsignarSIM_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var sim = button?.Tag as SIMViewModel;

            if (sim == null)
            {
                MessageBox.Show("Selecciona una SIM", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var ventana = new AsignarSIMProductoWindow(sim.SIMID, sim.NumeroSIM);
            ventana.ProductoAsignado += async () => await CargarSIMs();
            ventana.ShowDialog();
        }

        private async void BtnEliminarSIM_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var sim = button?.Tag as SIMViewModel;

            if (sim == null)
            {
                MessageBox.Show("Selecciona una SIM para eliminar", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (sim.Estado == "Asignada")
            {
                MessageBox.Show("No puedes eliminar una SIM que esta asignada a un producto.\n\nPrimero desasignala del producto.", 
                    "Operacion no permitida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirmacion = MessageBox.Show(
                "Estas seguro de eliminar la SIM '" + sim.NumeroSIM + "'?\n\nEsta accion NO se puede deshacer.",
                "Confirmar Eliminacion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmacion != MessageBoxResult.Yes)
                return;

            try
            {
                var resultado = await _simService.DeleteAsync(sim.SIMID);

                if (resultado)
                {
                    MessageBox.Show("SIM eliminada correctamente", "Exito", MessageBoxButton.OK, MessageBoxImage.Information);
                    await CargarSIMs();
                }
                else
                {
                    MessageBox.Show("No se pudo eliminar la SIM", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar la SIM: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnVolver_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            await CargarSIMs();
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

    public class SIMViewModel
    {
        public int SIMID { get; set; }
        public string NumeroSIM { get; set; }
        public int ProductoID { get; set; }
        public string ProductoNombre { get; set; }
        public string Estado { get; set; }
    }
}
