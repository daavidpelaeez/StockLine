using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WpfApp1.DTOs;
using WpfApp1.Services;

namespace WpfApp1.Views
{
    public partial class CategoriasWindow : Window
    {
        private readonly ICategoriaService _categoriaService;
        private ObservableCollection<CategoriaDto> _categorias;
        private ObservableCollection<CategoriaDto> _categoriasFiltradas;

        public CategoriasWindow()
        {
            InitializeComponent();
            
            _categoriaService = new CategoriaService();
            _categorias = new ObservableCollection<CategoriaDto>();
            _categoriasFiltradas = new ObservableCollection<CategoriaDto>();
            
            this.Loaded += CategoriasWindow_Loaded;
        }

        private async void CategoriasWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarCategorias();
        }

        private async System.Threading.Tasks.Task CargarCategorias(bool soloActivos = true)
        {
            try
            {
                var categorias = await _categoriaService.GetAllAsync(soloActivos ? "?activos=true" : null);
                
                _categorias.Clear();
                _categoriasFiltradas.Clear();
                
                foreach (var categoria in categorias)
                {
                    _categorias.Add(categoria);
                    _categoriasFiltradas.Add(categoria);
                }

                CategoriasGrid.ItemsSource = _categoriasFiltradas;
                ActualizarContador();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar categorías: {ex.Message}", 
                    "Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }

        private void BtnNuevaCategoria_Click(object sender, RoutedEventArgs e)
        {
            var ventanaCrear = new CrearEditarCategoriaWindow();
            if (ventanaCrear.ShowDialog() == true)
            {
                _ = CargarCategorias();
            }
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var categoria = button?.DataContext as CategoriaDto;
            
            if (categoria == null)
            {
                MessageBox.Show("Selecciona una categoría primero.", 
                    "Validación", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning);
                return;
            }

            var ventanaEditar = new CrearEditarCategoriaWindow(categoria);
            if (ventanaEditar.ShowDialog() == true)
            {
                _ = CargarCategorias();
            }
        }

        private async void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var categoria = button?.DataContext as CategoriaDto;
            
            if (categoria == null)
            {
                MessageBox.Show("Selecciona una categoria primero.", 
                    "Validacion", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                var productosCount = await _categoriaService.GetProductosCountByCategoriaAsync(categoria.CategoriaID);

                if (productosCount == -1)
                {
                    MessageBox.Show(
                        "No se pudo verificar si hay productos vinculados a esta categoria.\n\n" +
                        "Por seguridad, no se procedera con la eliminacion.",
                        "Error de Validacion",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                if (productosCount > 0)
                {
                    MessageBox.Show(
                        $"No se puede eliminar la categoria '{categoria.Nombre}'.\n\n" +
                        $"Hay {productosCount} producto(s) vinculado(s) a esta categoria.\n\n" +
                        "Por favor, reasigna o elimina estos productos antes de eliminar la categoria.",
                        "Categoria en Uso",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

               
                var confirmacion = MessageBox.Show(
                    $"¿Estas seguro de desactivar la categoria '{categoria.Nombre}'?\n\n" +
                    "Esta acción no se puede deshacer.",
                    "Confirmar Desactivación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirmacion != MessageBoxResult.Yes)
                    return;

                var resultado = await _categoriaService.DeleteAsync(categoria.CategoriaID);

                if (resultado)
                {
                    MessageBox.Show("Categoria desactivada correctamente.", 
                        "Éxito", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);
                    await CargarCategorias();
                }
                else
                {
                    MessageBox.Show("No se pudo desactivar la categoria.", 
                        "Error", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al desactivar la categoria:\n{ex.Message}", 
                    "Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
            }
        }

        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            var busqueda = txtBuscar.Text.ToLower();

            _categoriasFiltradas.Clear();

            var filtradas = string.IsNullOrWhiteSpace(busqueda)
                ? _categorias
                : _categorias.Where(c => c.Nombre.ToLower().Contains(busqueda));

            foreach (var categoria in filtradas)
            {
                _categoriasFiltradas.Add(categoria);
            }

            ActualizarContador();
        }

        private void ActualizarContador()
        {
            txtTotalCategorias.Text = $"Total: {_categoriasFiltradas.Count} categorías";
        }

        private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MaximizeWindow_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
                this.WindowState = WindowState.Normal;
            else
                this.WindowState = WindowState.Maximized;
        }
    }
}
