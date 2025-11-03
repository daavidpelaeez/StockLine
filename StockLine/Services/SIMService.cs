using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WpfApp1.DTOs;

namespace WpfApp1.Services
{
    public interface ISIMService
    {
        Task<List<SIMDTO>> GetAllAsync();
        Task<List<SIMDTO>> GetByProductoAsync(int productoId);
        Task<SIMDTO> GetByIdAsync(int id);
    }

    public class SIMService : ISIMService
    {
        private static readonly HttpClient client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5200/")
        };

        public async Task<List<SIMDTO>> GetAllAsync()
        {
            var response = await client.GetAsync("api/sims");
            
            if (!response.IsSuccessStatusCode)
                return new List<SIMDTO>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<SIMDTO>>(json);
        }

        public async Task<List<SIMDTO>> GetByProductoAsync(int productoId)
        {
            var response = await client.GetAsync($"api/sims/producto/{productoId}");
            
            if (!response.IsSuccessStatusCode)
                return new List<SIMDTO>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<SIMDTO>>(json);
        }

        public async Task<SIMDTO> GetByIdAsync(int id)
        {
            var response = await client.GetAsync($"api/sims/{id}");
            
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<SIMDTO>(json);
        }
    }
}
