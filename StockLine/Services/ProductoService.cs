using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WpfApp1.DTOs;

namespace WpfApp1.Services
{
    public interface IProductoService
    {
        Task<List<ProductoDto>> GetAllAsync();
        Task<ProductoDto> GetByIdAsync(int id);
    }

    public class ProductoService : IProductoService
    {
        private static readonly HttpClient client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5200/")
        };

        public async Task<List<ProductoDto>> GetAllAsync()
        {
            var response = await client.GetAsync("api/productos");
            
            if (!response.IsSuccessStatusCode)
                return new List<ProductoDto>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<ProductoDto>>(json);
        }

        public async Task<ProductoDto> GetByIdAsync(int id)
        {
            var response = await client.GetAsync($"api/productos/{id}");
            
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ProductoDto>(json);
        }
    }
}
