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
            _producto = new ProductoDto();
            txtProductoVentana.Text = "Nuevo Producto";
            CargarCategoriasAsync();
        }

        public AddProduct(ProductoDto producto)
        {
            InitializeComponent();
            _producto = producto;

            // Cargar campos
            NombreBox.Text = _producto.Nombre;
            DescripcionBox.Text = _producto.Descripcion;
            CantidadBox.Text = _producto.Stock.ToString();

            txtProductoVentana.Text = "Editar Producto";
            
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
                // Error silencioso si no hay foto
            }
        }

        private async void Guardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validar campos básicos
                if (string.IsNullOrWhiteSpace(NombreBox.Text))
                {
                    MessageBox.Show("El nombre es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(CantidadBox.Text, out int stock))
                {
                    MessageBox.Show("La cantidad debe ser un número válido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (CategoriaComboBox.SelectedValue == null)
                {
                    MessageBox.Show("Debes seleccionar una categoría.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:5200/");
                    client.Timeout = TimeSpan.FromMinutes(5);

                    HttpResponseMessage response = null;
                    int productoId = 0;

                    bool tieneFotoLocal = !string.IsNullOrEmpty(_producto.Foto) && File.Exists(_producto.Foto);

                    System.Diagnostics.Debug.WriteLine($"Tiene foto local: {tieneFotoLocal}");

                    if (tieneFotoLocal)
                    {
                        // Preparar contenido del archivo
                        var fileBytes = File.ReadAllBytes(_producto.Foto);
                        var fileContent = new ByteArrayContent(fileBytes);

                        string extension = System.IO.Path.GetExtension(_producto.Foto).ToLower();
                        string contentType = "image/jpeg";
                        if (extension == ".png") contentType = "image/png";
                        else if (extension == ".webp") contentType = "image/webp";
                        else if (extension == ".gif") contentType = "image/gif";
                        else if (extension == ".jpg" || extension == ".jpeg") contentType = "image/jpeg";

                        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

                        if (_producto.ProductoID > 0)
                        {
                            // El servidor puede no soportar PUT multipart. Primero actualizamos los datos sin la foto (JSON PUT)
                            var productoDataNoFoto = new
                            {
                                ProductoID = _producto.ProductoID,
                                Nombre = NombreBox.Text.Trim(),
                                Descripcion = string.IsNullOrWhiteSpace(DescripcionBox.Text) ? "" : DescripcionBox.Text.Trim(),
                                Stock = stock,
                                CategoriaID = (int)CategoriaComboBox.SelectedValue,
                                Foto = "default.png"
                            };

                            var jsonSettings = new Newtonsoft.Json.JsonSerializerSettings
                            {
                                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
                                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                                Formatting = Newtonsoft.Json.Formatting.None
                            };

                            var jsonNoFoto = Newtonsoft.Json.JsonConvert.SerializeObject(productoDataNoFoto, jsonSettings);
                            var jsonContent = new StringContent(jsonNoFoto, System.Text.Encoding.UTF8, "application/json");

                            System.Diagnostics.Debug.WriteLine($"Actualizando producto (sin foto) ID: {_producto.ProductoID}");
                            var putResponse = await client.PutAsync($"api/productos/{_producto.ProductoID}", jsonContent);

                            // Si la actualización falla, retornamos el error
                            if (!putResponse.IsSuccessStatusCode)
                            {
                                response = putResponse;
                            }
                            else
                            {
                                productoId = _producto.ProductoID;

                                // Luego subimos la foto por separado al endpoint de upload
                                using (var uploadForm = new MultipartFormDataContent())
                                {
                                    uploadForm.Add(fileContent, "Foto", System.IO.Path.GetFileName(_producto.Foto));

                                    System.Diagnostics.Debug.WriteLine($"Subiendo foto al endpoint: api/productos/upload/{productoId}");
                                    var uploadResponse = await client.PostAsync($"api/productos/upload/{productoId}", uploadForm);
                                    response = uploadResponse;
                                }
                            }
                        }
                        else
                        {
                            // Crear nuevo producto con multipart/form-data (incluye foto en la misma petición)
                            using (var form = new MultipartFormDataContent())
                            {
                                form.Add(new StringContent(NombreBox.Text.Trim()), "Nombre");
                                form.Add(new StringContent(string.IsNullOrWhiteSpace(DescripcionBox.Text) ? "" : DescripcionBox.Text.Trim()), "Descripcion");
                                form.Add(new StringContent(stock.ToString()), "Stock");
                                form.Add(new StringContent(((int)CategoriaComboBox.SelectedValue).ToString()), "CategoriaID");

                                form.Add(fileContent, "Foto", System.IO.Path.GetFileName(_producto.Foto));

                                System.Diagnostics.Debug.WriteLine("Enviando multipart POST a api/productos");
                                response = await client.PostAsync("api/productos", form);
                            }
                        }
                    }
                    else
                    {
                        // Sin foto: enviar JSON con los campos necesarios
                        var productoData = new
                        {
                            ProductoID = _producto.ProductoID,
                            Nombre = NombreBox.Text.Trim(),
                            Descripcion = string.IsNullOrWhiteSpace(DescripcionBox.Text) ? "" : DescripcionBox.Text.Trim(),
                            Stock = stock,
                            CategoriaID = (int)CategoriaComboBox.SelectedValue,
                            Foto = "default.png"
                        };

                        var jsonSettings = new Newtonsoft.Json.JsonSerializerSettings
                        {
                            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
                            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                            Formatting = Newtonsoft.Json.Formatting.None
                        };

                        var json = Newtonsoft.Json.JsonConvert.SerializeObject(productoData, jsonSettings);
                        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                        if (_producto.ProductoID > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"Enviando JSON PUT a api/productos/{_producto.ProductoID}: {json}");
                            response = await client.PutAsync($"api/productos/{_producto.ProductoID}", content);
                            productoId = _producto.ProductoID;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Enviando JSON POST a api/productos: {json}");
                            response = await client.PostAsync("api/productos", content);
                        }
                    }

                    var responseBody = response != null ? await response.Content.ReadAsStringAsync() : string.Empty;
                    System.Diagnostics.Debug.WriteLine($"Código de respuesta: {response?.StatusCode}");
                    System.Diagnostics.Debug.WriteLine($"Cuerpo de respuesta: {responseBody}");

                    if (response == null || !response.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"Error al guardar el producto:\n\nCódigo: {response?.StatusCode}\nDetalle: {responseBody}", 
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Intentar obtener el ID del producto guardado
                    try
                    {
                        var jsonSettingsResp = new Newtonsoft.Json.JsonSerializerSettings
                        {
                            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
                            NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
                        };

                        var productoGuardado = Newtonsoft.Json.JsonConvert.DeserializeObject<ProductoDto>(responseBody, jsonSettingsResp);
                        if (productoGuardado != null && productoGuardado.ProductoID > 0)
                        {
                            productoId = productoGuardado.ProductoID;
                        }
                        else
                        {
                            // intentar extraer manualmente
                            dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject(responseBody);
                            if (obj != null)
                            {
                                if (obj.ProductoID != null) productoId = (int)obj.ProductoID;
                                else if (obj.productoID != null) productoId = (int)obj.productoID;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"No se pudo obtener ProductoID: {ex.Message}");
                    }

                    System.Diagnostics.Debug.WriteLine($"Producto guardado con ID: {productoId}");

                    // Si enviamos multipart con foto ya se subió en la misma petición; no hacer upload separado
                }

                MessageBox.Show("Producto guardado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (HttpRequestException httpEx)
            {
                System.Diagnostics.Debug.WriteLine($"Error HTTP: {httpEx.Message}");
                MessageBox.Show($"Error de conexión con el servidor:\n\n{httpEx.Message}\n\nPor favor, verifica que el servidor esté ejecutándose en http://localhost:5200", "Error de Conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error general: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                MessageBox.Show($"Error al guardar el producto:\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    CategoriaComboBox.DisplayMemberPath = "Nombre";
                    CategoriaComboBox.SelectedValuePath = "CategoriaID";

                    // Seleccionar categoría del producto si existe
                    if (_producto != null && _producto.CategoriaID.HasValue)
                    {
                        CategoriaComboBox.SelectedValue = _producto.CategoriaID;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cargando categorías:\n\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarDetallesRapidos()
        {
            if (_producto == null) return;

            QuickNombre.Text = _producto.Nombre ?? "—";
            QuickCantidad.Text = _producto.Stock.ToString();
            QuickDescripcion.Text = _producto.Descripcion ?? "—";
        }

        private void SeleccionarFoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.webp";
            if (ofd.ShowDialog() == true)
            {
                PreviewImage.Source = new BitmapImage(new Uri(ofd.FileName));
                PreviewHint.Visibility = Visibility.Collapsed;
                FotoNombre.Text = System.IO.Path.GetFileName(ofd.FileName);

                // Guardamos temporalmente la ruta local
                _producto.Foto = ofd.FileName;
            }
        }

        private void LimpiarFoto_Click(object sender, RoutedEventArgs e)
        {
            FotoNombre.Text = "";
            PreviewImage.Source = null;
            PreviewHint.Visibility = Visibility.Visible;
            _producto.Foto = null;
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
                    FotoNombre.Text = System.IO.Path.GetFileName(file);
                    PreviewImage.Source = new BitmapImage(new Uri(file, UriKind.Absolute));
                    PreviewHint.Visibility = Visibility.Collapsed;
                    _producto.Foto = file;
                }
            }
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        // Eventos para actualizar vista previa en tiempo real
        private void NombreBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            QuickNombre.Text = string.IsNullOrWhiteSpace(NombreBox.Text) ? "—" : NombreBox.Text;
        }

        private void DescripcionBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            QuickDescripcion.Text = string.IsNullOrWhiteSpace(DescripcionBox.Text) ? "—" : DescripcionBox.Text;
        }

        private void CantidadBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            QuickCantidad.Text = string.IsNullOrWhiteSpace(CantidadBox.Text) ? "—" : CantidadBox.Text;
        }
    }
}
