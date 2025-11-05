using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using WpfApp1.DTOs;

namespace WpfApp1.Views
{
    public partial class CrearMovimientoWindow : Window
    {
        public CrearMovimientoWindow()
        {
            InitializeComponent();
            Loaded += CrearMovimientoWindow_Loaded;
        }

        private async void CrearMovimientoWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarProductos();
        }

        private async Task CargarProductos()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:5200/");
                    var res = await client.GetAsync("api/productos");
                    if (res.IsSuccessStatusCode)
                    {
                        var json = await res.Content.ReadAsStringAsync();
                        var productos = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ProductoDto>>(json);
                        cbProducto.ItemsSource = productos;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error cargando productos: " + ex.Message);
            }
        }

        private async void Crear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cbProducto.SelectedValue == null)
                {
                    MessageBox.Show("Debes seleccionar un producto.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtCantidad.Text))
                {
                    MessageBox.Show("Debes indicar la cantidad.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(txtCantidad.Text, out int cantidad) || cantidad <= 0)
                {
                    MessageBox.Show("La cantidad debe ser un número mayor que cero.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (cbTipo.SelectedItem == null)
                {
                    MessageBox.Show("Debes seleccionar el tipo de movimiento.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string tipo = cbTipo.Text;
                if (tipo == "Salida")
                {
                    if (cbProducto.SelectedItem is ProductoDto p && cantidad > p.Stock)
                    {
                        var confirm = MessageBox.Show($"La cantidad supera el stock actual ({p.Stock}). ¿Continuar?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (confirm != MessageBoxResult.Yes) return;
                    }
                }

                if (string.IsNullOrWhiteSpace(txtUsuario.Text))
                {
                    MessageBox.Show("Debes indicar el ID de usuario.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!int.TryParse(txtUsuario.Text, out int usuarioId) || usuarioId <= 0)
                {
                    MessageBox.Show("El ID de usuario debe ser un número válido.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://localhost:5200/");
                    var dto = new {
                        productoID = (int)cbProducto.SelectedValue,
                        cantidad = cantidad,
                        tipoMovimiento = tipo,
                        usuarioID = usuarioId,
                        observaciones = txtObservaciones.Text?.Trim() ?? ""
                    };

                    var json = Newtonsoft.Json.JsonConvert.SerializeObject(dto);
                    var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                    var res = await client.PostAsync("api/movimientosstock", content);
                    var body = await res.Content.ReadAsStringAsync();
                    if (!res.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Error creando movimiento: " + body);
                        return;
                    }

                    MessageBox.Show("Movimiento creado.");
                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creando movimiento: " + ex.Message);
            }
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
