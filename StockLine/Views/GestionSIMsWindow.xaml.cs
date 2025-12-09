using System;
using System.Collections.Generic;
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
        private List<SIMDTO> _sims;
        private List<SIMDTO> _simsFiltradas;

        public GestionSIMsWindow()
        {
            InitializeComponent();
            _simService = new SIMService();
            _sims = new List<SIMDTO>();
            _simsFiltradas = new List<SIMDTO>();
            dgSIMs.ItemsSource = _simsFiltradas;
            this.Loaded += GestionSIMsWindow_Loaded;
            txtBuscar.TextChanged += TxtBuscar_TextChanged;
            cbFiltroEstado.SelectionChanged += CbFiltroEstado_SelectionChanged;
            btnNuevaSIM.Click += BtnNuevaSIM_Click;
            btnActualizar.Click += BtnActualizar_Click;
        }

        private async void GestionSIMsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await RecargarSIMs();
        }

        private async System.Threading.Tasks.Task RecargarSIMs()
        {
            try
            {
                var sims = await _simService.GetAllAsync();
                _sims = sims ?? new List<SIMDTO>();


                AplicarFiltros();
                dgSIMs.ItemsSource = null;
                dgSIMs.ItemsSource = _simsFiltradas;
                dgSIMs.Items.Refresh();
                txtTotalSIMs.Text = $"Total de SIMs: {_simsFiltradas.Count}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar SIMs: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AplicarFiltros()
        {
            IEnumerable<SIMDTO> resultado = _sims;

            if (!string.IsNullOrWhiteSpace(txtBuscar.Text))
            {
                var busqueda = txtBuscar.Text.ToLower();
                resultado = resultado.Where(s =>
                    (s.NumeroSIM ?? "").ToLower().Contains(busqueda)
                    || (s.ProductoNombre ?? "").ToLower().Contains(busqueda)
                    || (s.Ubicacion ?? "").ToLower().Contains(busqueda)
                );
            }

            switch (cbFiltroEstado.SelectedIndex)
            {
                case 1: 
                    resultado = resultado.Where(s => s.ProductoID == null);
                    break;
                case 2: 
                    resultado = resultado.Where(s => s.ProductoID != null);
                    break;
            }

            _simsFiltradas = resultado.ToList();
        }

        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            AplicarFiltros();
            dgSIMs.ItemsSource = null;
            dgSIMs.ItemsSource = _simsFiltradas;
            txtTotalSIMs.Text = $"Total de SIMs: {_simsFiltradas.Count}";
        }

        private void CbFiltroEstado_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AplicarFiltros();
            dgSIMs.ItemsSource = null;
            dgSIMs.ItemsSource = _simsFiltradas;
            txtTotalSIMs.Text = $"Total de SIMs: {_simsFiltradas.Count}";
        }

        private void BtnNuevaSIM_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new CrearEditarSIMWindow();
            ventana.SIMGuardada += async () => await RecargarSIMs();
            ventana.ShowDialog();
        }

        private void BtnEditarSIM_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is SIMDTO sim)
            {
                var ventana = new CrearEditarSIMWindow(sim.SIMID);
                ventana.SIMGuardada += async () => await RecargarSIMs();
                ventana.ShowDialog();
            }
            else
            {
                MessageBox.Show("Selecciona una SIM para editar", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void BtnAsignarSIM_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is SIMDTO sim)
            {
                if (sim.ProductoID != null)
                {
                    var confirmacion = MessageBox.Show($"¿Desea desasignar la SIM '{sim.NumeroSIM}'?", "Desasignar SIM", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (confirmacion != MessageBoxResult.Yes) return;
                    var (exito, error) = await _simService.DesasignarProductoAsync(sim.SIMID);
                    if (exito)
                    {
                        await RecargarSIMs();
                        MessageBox.Show("SIM desasignada correctamente y devuelta a 'En almacén'", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Error al desasignar SIM: {error}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    var ventana = new AsignarSIMProductoWindow(sim.SIMID, sim.NumeroSIM);
                    ventana.ProductoAsignado += async () => await RecargarSIMs();
                    ventana.ShowDialog();
                }
            }
            else
            {
                MessageBox.Show("Selecciona una SIM", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void BtnEliminarSIM_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is SIMDTO sim)
            {
                var confirmacion = MessageBox.Show($"¿Estas seguro de eliminar la SIM '{sim.NumeroSIM}'? Esta acción NO se puede deshacer.", "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (confirmacion != MessageBoxResult.Yes) return;
                try
                {
                    var (exito, error) = await _simService.DeleteAsync(sim.SIMID);
                    if (exito)
                    {
                        MessageBox.Show("SIM eliminada correctamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                        await RecargarSIMs();
                    }
                    else
                    {
                        MessageBox.Show($"No se pudo eliminar la SIM: {error}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar la SIM: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Selecciona una SIM para eliminar", "Validacion", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            await RecargarSIMs();
        }

        private void MinimizeWindow_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
        private void MaximizeWindow_Click(object sender, RoutedEventArgs e) => this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        private void CloseWindow_Click(object sender, RoutedEventArgs e) => this.Close();
        private void BtnVolver_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}
