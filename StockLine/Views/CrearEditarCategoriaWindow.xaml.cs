using System;
using System.Windows;
using WpfApp1.DTOs;
using WpfApp1.Services;

namespace WpfApp1.Views
{
    public partial class CrearEditarCategoriaWindow : Window
    {
        private readonly ICategoriaService _categoriaService;
        private CategoriaDto _categoria;
        private bool _esEdicion;

        // Constructor para CREAR
        public CrearEditarCategoriaWindow()
        {
            InitializeComponent();
            
            _categoriaService = new CategoriaService();
            _categoria = new CategoriaDto();
            _esEdicion = false;
            
            txtTitulo.Text = "Nueva Categoría";
            btnGuardar.Content = "Crear";
        }

        // Constructor para EDITAR
        public CrearEditarCategoriaWindow(CategoriaDto categoria)
        {
            InitializeComponent();
            
            _categoriaService = new CategoriaService();
            _categoria = categoria;
            _esEdicion = true;
            
            txtTitulo.Text = "Editar Categoría";
            btnGuardar.Content = "Actualizar";
            txtNombre.Text = categoria.Nombre;
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Validar nombre
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                txtError.Visibility = Visibility.Visible;
                txtNombre.BorderBrush = System.Windows.Media.Brushes.Red;
                return;
            }

            txtError.Visibility = Visibility.Collapsed;
            txtNombre.BorderBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)FindResource("BorderLightColor"));

            try
            {
                btnGuardar.IsEnabled = false;

                _categoria.Nombre = txtNombre.Text.Trim();

                bool resultado;

                if (_esEdicion)
                {
                    // ACTUALIZAR
                    resultado = await _categoriaService.UpdateAsync(_categoria);
                }
                else
                {
                    // CREAR
                    var categoriaCreada = await _categoriaService.CreateAsync(_categoria);
                    resultado = categoriaCreada != null;
                }

                if (resultado)
                {
                    string mensaje = _esEdicion 
                        ? "Categoría actualizada correctamente" 
                        : "Categoría creada correctamente";

                    MessageBox.Show(mensaje, 
                        "Éxito", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Information);

                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("No se pudo guardar la categoría.", 
                        "Error", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);
                    btnGuardar.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar la categoría:\n{ex.Message}", 
                    "Error", 
                    MessageBoxButton.OK, 
                    MessageBoxImage.Error);
                btnGuardar.IsEnabled = true;
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void txtNombre_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                txtError.Visibility = Visibility.Collapsed;
                txtNombre.BorderBrush = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)FindResource("BorderLightColor"));
            }
        }
    }
}
