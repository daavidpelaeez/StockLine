using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WpfApp1.DTOs;

namespace WpfApp1.Services
{
    public class CategoriaService : ICategoriaService
    {
        private static readonly HttpClient client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5200/")
        };

        public async Task<List<CategoriaDto>> GetAllAsync()
        {
            var response = await client.GetAsync("api/categorias");
            
            if (!response.IsSuccessStatusCode)
                return new List<CategoriaDto>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<CategoriaDto>>(json);
        }

        public async Task<CategoriaDto> GetByIdAsync(int id)
        {
            var response = await client.GetAsync($"api/categorias/{id}");
            
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CategoriaDto>(json);
        }

        public async Task<CategoriaDto> CreateAsync(CategoriaDto categoria)
        {
            var json = JsonConvert.SerializeObject(categoria);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/categorias", content);
            
            if (!response.IsSuccessStatusCode)
                return null;

            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CategoriaDto>(responseJson);
        }

        public async Task<bool> UpdateAsync(CategoriaDto categoria)
        {
            var json = JsonConvert.SerializeObject(categoria);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"api/categorias/{categoria.CategoriaID}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await client.DeleteAsync($"api/categorias/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<int> GetProductosCountByCategoriaAsync(int categoriaId)
        {
            try
            {
                var response = await client.GetAsync($"api/categorias/{categoriaId}/productos/count");
                
                if (!response.IsSuccessStatusCode)
                    return -1; // Error al obtener el conteo

                var json = await response.Content.ReadAsStringAsync();
                
                // Si la API devuelve solo un número
                if (int.TryParse(json, out int count))
                    return count;
                
                // Si la API devuelve un objeto JSON como {"count": 5}
                var result = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);
                if (result != null && result.ContainsKey("count"))
                    return result["count"];
                
                return -1;
            }
            catch
            {
                return -1; // Error en la petición
            }
        }
    }
}
