using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfApp1.DTOs;
using WpfApp1.Models;

namespace WpfApp1.Views
{
    /// <summary>
    /// Interaction logic for AddProduct.xaml
    /// </summary>
    public partial class AddProduct : Window
    {
        private ProductoDto productoEdicion;
        private string rutaFoto;
        private ProductoDto _producto;
        public AddProduct()
        {
            InitializeComponent();
        }

        public AddProduct(ProductoDto producto)
        {
            InitializeComponent();
            _producto = producto;

            // Cargar campos
            NombreBox.Text = _producto.Nombre;
            DescripcionBox.Text = _producto.Descripcion;
            CantidadBox.Text = _producto.Stock.ToString();

            CargarFotoExistenteAsync();
            CargarDetallesRapidos();
            CargarCategoriasAsync();
        }

        private async Task CargarFotoExistenteAsync()
        {
            if (_producto == null || _producto.ProductoID == 0)
                return;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:5200/");
                    var response = await client.GetAsync($"api/productos/photo/{_producto.ProductoID}");
                    if (response.IsSuccessStatusCode)
                    {
                        var bytes = await response.Content.ReadAsByteArrayAsync();
                        if (bytes != null && bytes.Length > 0)
                        {
                            BitmapImage image = new BitmapImage();
                            using (var ms = new MemoryStream(bytes))
                            {
                                image.BeginInit();
                                image.CacheOption = BitmapCacheOption.OnLoad;
                                image.StreamSource = ms;
                                image.EndInit();
                                image.Freeze();
                            }
                            PreviewImage.Source = image;
                            PreviewHint.Visibility = Visibility.Collapsed;


                            _producto.Foto = null; 
                        }
                    }
                }
            }
            catch
            {

            }
        }


        private async void Guardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validar campos básicos
                if (string.IsNullOrWhiteSpace(NombreBox.Text))
                {
                    MessageBox.Show("El nombre es obligatorio.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(CantidadBox.Text, out int stock))
                {
                    MessageBox.Show("Cantidad inválida.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Actualizar DTO con los valores del formulario
                _producto.Nombre = NombreBox.Text.Trim();
                _producto.Descripcion = DescripcionBox.Text.Trim();
                _producto.Stock = stock;
                _producto.CategoriaID = CategoriaComboBox.SelectedValue != null
                    ? (int)CategoriaComboBox.SelectedValue
                    : (int?)null;

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:5200/"); // URL de tu API

                    // 1️⃣ Guardar producto
                    var json = System.Text.Json.JsonSerializer.Serialize(_producto);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    var response = await client.PutAsync("api/productos/" + _producto.ProductoID, content);
                    response.EnsureSuccessStatusCode();

                    // 2️⃣ Actualizar _producto con la respuesta del servidor
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    _producto = Newtonsoft.Json.JsonConvert.DeserializeObject<ProductoDto>(jsonResponse);

                    // 3️⃣ Actualizar el ComboBox con la categoría actual
                    if (_producto.CategoriaID.HasValue)
                    {
                        CategoriaComboBox.SelectedValue = _producto.CategoriaID;
                    }

                    // 2️⃣ Subir la foto si se cambió (si _producto.Foto es ruta local)
                    if (!string.IsNullOrEmpty(_producto.Foto) && File.Exists(_producto.Foto))
                    {
                        using (var form = new MultipartFormDataContent())
                        using (var fileStream = File.OpenRead(_producto.Foto))
                        {
                            var fileContent = new StreamContent(fileStream);
                            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg"); // o detectar según extensión
                            form.Add(fileContent, "Foto", System.IO.Path.GetFileName(_producto.Foto));

                            var uploadResponse = await client.PostAsync("api/productos/upload/" + _producto.ProductoID, form);
                            uploadResponse.EnsureSuccessStatusCode();
                        }
                    }
                }

                MessageBox.Show("Producto guardado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true; // cierra la ventana y devuelve true
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar el producto:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async Task CargarCategoriasAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:5200");

                    var response = await client.GetAsync("/api/Categorias");
                    response.EnsureSuccessStatusCode();

                    var json = await response.Content.ReadAsStringAsync();
                    var categorias = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CategoriaDto>>(json);

                    CategoriaComboBox.ItemsSource = categorias;
                    CategoriaComboBox.DisplayMemberPath = "Nombre";  // lo que se muestra
                    CategoriaComboBox.SelectedValuePath = "CategoriaID"; // valor seleccionado

                    // **Selecciona la categoría del producto después de asignar ItemsSource**
                    if (_producto != null && _producto.CategoriaID.HasValue)
                    {
                        CategoriaComboBox.SelectedValue = _producto.CategoriaID;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cargando categorías: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void CargarDetallesRapidos()
        {
            if (_producto == null) return;

            QuickNombre.Text = _producto.Nombre ?? "—";
            QuickCantidad.Text = _producto.Stock.ToString();
            QuickDescripcion.Text = _producto.Descripcion ?? "-";
            txtProductoVentana.Text = _producto.Nombre ?? "Producto";
        }


        private void SeleccionarFoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Images|*.jpg;*.jpeg;*.png;*.webp";
            if (ofd.ShowDialog() == true)
            {
                PreviewImage.Source = new BitmapImage(new Uri(ofd.FileName));
                PreviewHint.Visibility = Visibility.Collapsed;

                // Guardamos temporalmente la ruta local
                _producto.Foto = ofd.FileName;
            }
        }

        private void LimpiarFoto_Click(object sender, RoutedEventArgs e)
        {
            FotoNombre.Text = "";
            PreviewImage.Source = null;
            PreviewHint.Visibility = Visibility.Visible;
        }

        private void ImageDrop_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;

            e.Handled = true;
        }

        private void ImageDrop_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    string file = files[0];
                    FotoNombre.Text = file;
                    PreviewImage.Source = new BitmapImage(new Uri(file, UriKind.Absolute));
                    PreviewHint.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
