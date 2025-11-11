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
        Task<SIMDTO> CreateAsync(SIMDTO sim);
        Task<bool> UpdateAsync(int id, SIMDTO sim);
        Task<(bool, string)> DeleteAsync(int id);
        Task<(bool, string)> AsignarProductoAsync(int simId, int productoId);
        Task<(bool, string)> DesasignarProductoAsync(int simId);
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

        public async Task<SIMDTO> CreateAsync(SIMDTO sim)
        {
            var json = JsonConvert.SerializeObject(sim);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/sims", content);
            
            if (!response.IsSuccessStatusCode)
                return null;

            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<SIMDTO>(responseJson);
        }

        public async Task<bool> UpdateAsync(int id, SIMDTO sim)
        {
            var json = JsonConvert.SerializeObject(sim);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"api/sims/{id}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<(bool, string)> DeleteAsync(int id)
        {
            var response = await client.DeleteAsync($"api/sims/{id}");
            var error = await response.Content.ReadAsStringAsync();
            return (response.IsSuccessStatusCode, error);
        }

        public async Task<(bool, string)> AsignarProductoAsync(int simId, int productoId)
        {
            var json = JsonConvert.SerializeObject(productoId); // solo el número
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"api/sims/{simId}/asignar-producto")
            {
                Content = content
            };
            var response = await client.SendAsync(request);
            var error = await response.Content.ReadAsStringAsync();
            return (response.IsSuccessStatusCode, error);
        }

        public async Task<(bool, string)> DesasignarProductoAsync(int simId)
        {
            // Usar PUT, endpoint correcto y body 'null' como texto plano
            var json = "null";
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"api/sims/{simId}/desasignar", content);
            var error = await response.Content.ReadAsStringAsync();
            return (response.IsSuccessStatusCode, error);
        }
    }
}
