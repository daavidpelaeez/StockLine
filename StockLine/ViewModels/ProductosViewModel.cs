using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using WpfApp1.DTOs;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace WpfApp1.ViewModels
{
    public class ProductosViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _client = new HttpClient();
        private readonly string baseUrl = "http://localhost:5200/api/Productos";
        private readonly string categoriasUrl = "http://localhost:5200/api/Categorias";

        public ObservableCollection<ProductoDto> Productos { get; set; }
        public ObservableCollection<ProductoDto> ProductosFiltrados { get; set; }
        public ObservableCollection<CategoriaDto> Categorias { get; set; }

        public ProductosViewModel()
        {
            Productos = new ObservableCollection<ProductoDto>();
            ProductosFiltrados = new ObservableCollection<ProductoDto>();
            Categorias = new ObservableCollection<CategoriaDto>();
        }

        private int totalProductos;
        public int TotalProductos
        {
            get { return totalProductos; }
            set { totalProductos = value; OnPropertyChanged("TotalProductos"); }
        }

        private int criticos;
        public int Criticos
        {
            get { return criticos; }
            set { criticos = value; OnPropertyChanged("Criticos"); }
        }

        private int unidades;
        public int Unidades
        {
            get { return unidades; }
            set { unidades = value; OnPropertyChanged("Unidades"); }
        }

        
        private CategoriaDto _categoriaSeleccionada;
        public CategoriaDto CategoriaSeleccionada
        {
            get { return _categoriaSeleccionada; }
            set
            {
                _categoriaSeleccionada = value;
                FiltrarProductos();
                OnPropertyChanged("CategoriaSeleccionada");
            }
        }

        private bool _soloCriticos;
        public bool SoloCriticos
        {
            get { return _soloCriticos; }
            set
            {
                _soloCriticos = value;
                FiltrarProductos();
                OnPropertyChanged("SoloCriticos");
            }
        }

        #region Cargar Datos desde API
        public async Task CargarProductosAsync()
        {
            try
            {
                List<ProductoDto> lista = await _client.GetFromJsonAsync<List<ProductoDto>>(baseUrl);
                Productos.Clear();
                if (lista != null)
                {
                    foreach (ProductoDto p in lista)
                    {
                        p.Stock = p.Stock;
                        if (p.Nombre == null) p.Nombre = "";
                        if (p.Descripcion == null) p.Descripcion = "";
                        if (p.Foto == null) p.Foto = "default.png";

                        Productos.Add(p);
                    }
                }

                ActualizarKPI();
                FiltrarProductos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar productos: " + ex.Message);
            }
        }

        public async Task CargarCategoriasAsync()
        {
            try
            {
                List<CategoriaDto> lista = await _client.GetFromJsonAsync<List<CategoriaDto>>(categoriasUrl);
                Categorias.Clear();

                
                Categorias.Add(new CategoriaDto { CategoriaID = 0, Nombre = "Todas" });

                if (lista != null)
                {
                    foreach (CategoriaDto c in lista)
                    {
                        Categorias.Add(c);
                    }
                }

                if (Categorias.Count > 0)
                    CategoriaSeleccionada = Categorias[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar categorías: " + ex.Message);
            }
        }
        #endregion

        #region Filtrar
        private void FiltrarProductos()
        {
            ProductosFiltrados.Clear();
            IEnumerable<ProductoDto> query = Productos;

            if (CategoriaSeleccionada != null && CategoriaSeleccionada.CategoriaID != 0)
                query = System.Linq.Enumerable.Where(query, p => p.CategoriaID == CategoriaSeleccionada.CategoriaID);

            if (SoloCriticos)
                query = System.Linq.Enumerable.Where(query, p => p.Stock < 10);

            foreach (ProductoDto p in query)
                ProductosFiltrados.Add(p);
        }

        public void AplicarBusqueda(string textoBusqueda)
        {
            ProductosFiltrados.Clear();
            IEnumerable<ProductoDto> query = Productos;

            
            if (CategoriaSeleccionada != null && CategoriaSeleccionada.CategoriaID != 0)
                query = System.Linq.Enumerable.Where(query, p => p.CategoriaID == CategoriaSeleccionada.CategoriaID);

            
            if (SoloCriticos)
                query = System.Linq.Enumerable.Where(query, p => p.Stock < 10);

            
            if (!string.IsNullOrEmpty(textoBusqueda))
            {
                query = System.Linq.Enumerable.Where(query, p =>
                    p.Nombre.ToLower().Contains(textoBusqueda) ||
                    p.Descripcion.ToLower().Contains(textoBusqueda) ||
                    p.ProductoID.ToString().Contains(textoBusqueda)
                );
            }

            foreach (ProductoDto p in query)
                ProductosFiltrados.Add(p);
        }

        public void LimpiarFiltros()
        {
            if (Categorias.Count > 0)
                CategoriaSeleccionada = Categorias[0];
            SoloCriticos = false;
        }
        #endregion

        #region Importar / Exportar CSV
        public async Task ImportarDesdeCsvAsync(string filePath)
        {
            try
            {
                string[] lines = System.IO.File.ReadAllLines(filePath, Encoding.UTF8);
                if (lines.Length < 2)
                {
                    MessageBox.Show("Archivo vacío o sin cabecera.", "Importar", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Productos.Clear();

                for (int i = 1; i < lines.Length; i++)
                {
                    string[] values = ParseCsvLine(lines[i]);
                    if (values.Length >= 4) 
                    {
                        ProductoDto producto = new ProductoDto();
                        int id;
                        int cant;
                        producto.ProductoID = int.TryParse(values[0], out id) ? id : 0;
                        producto.Nombre = values[1] ?? "";
                        producto.Descripcion = values[2] ?? "";
                        producto.Stock = int.TryParse(values[3], out cant) ? cant : 0;
                        producto.Foto = (values.Length > 4 && !string.IsNullOrWhiteSpace(values[4])) ? values[4] : "default.png";

                        await GuardarProductoEnBdAsync(producto, 0);
                        Productos.Add(producto);
                    }
                }

                ActualizarKPI();
                FiltrarProductos();
                MessageBox.Show("Importación completada y guardada en BD.", "Importar", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error importando: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ExportarAExcel(string filePath)
        {
            try
            {
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(filePath, false, Encoding.UTF8))
                {
                    string[] headers = new string[] { "Id", "Nombre", "Descripcion", "Stock", "Foto", "CategoriaID", "CategoriaNombre" };
                    writer.WriteLine(string.Join(",", headers));

                    foreach (ProductoDto p in Productos)
                    {
                        string[] row = new string[]
                        {
                            EscapeCsv(p.ProductoID.ToString()),
                            EscapeCsv(p.Nombre),
                            EscapeCsv(p.Descripcion),
                            EscapeCsv(p.Stock.ToString()),
                            EscapeCsv(p.Foto),
                            EscapeCsv(p.CategoriaID.HasValue ? p.CategoriaID.Value.ToString() : ""),
                            EscapeCsv(p.CategoriaNombre)
                        };
                        writer.WriteLine(string.Join(",", row));
                    }
                }

                MessageBox.Show("Exportación completada.", "Exportar", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exportando: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string[] ParseCsvLine(string line)
        {
            List<string> values = new List<string>();
            bool inQuotes = false;
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"'); i++;
                    }
                    else inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    values.Add(sb.ToString()); sb.Clear();
                }
                else sb.Append(c);
            }
            values.Add(sb.ToString());
            return values.ToArray();
        }

        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            {
                value = value.Replace("\"", "\"\"");
                return "\"" + value + "\"";
            }
            return value;
        }
        #endregion

        #region Guardar Producto en BD
        private async Task GuardarProductoEnBdAsync(ProductoDto producto, int proveedorId)
        {
            try
            {
                var payload = new
                {
                    producto.ProductoID,
                    producto.Nombre,
                    producto.Descripcion,
                    Foto = producto.Foto,
                    ProveedorId = proveedorId,
                    CategoriaId = producto.CategoriaID
                };

                HttpResponseMessage response = await _client.PostAsJsonAsync(baseUrl, payload);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error guardando " + producto.Nombre + " en BD: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region KPI
        private void ActualizarKPI()
        {
            TotalProductos = Productos.Count;
            Criticos = Productos.Count(p => p.Stock < 10); 
            Unidades = Productos.Sum(p => p.Stock);
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string nombre)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(nombre));
        }
        #endregion
    }
}
