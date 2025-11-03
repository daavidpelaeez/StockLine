using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WpfApp1.Services
{
    public interface IComercialService
    {
        Task<List<DTOs.ComercialDTO>> GetAllAsync();
        Task<DTOs.ComercialDTO> GetByIdAsync(int id);
    }

    public class ComercialService : IComercialService
    {
        private static readonly HttpClient client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5200/")
        };

        public async Task<List<DTOs.ComercialDTO>> GetAllAsync()
        {
            var response = await client.GetAsync("api/comerciales");
            
            if (!response.IsSuccessStatusCode)
                return new List<DTOs.ComercialDTO>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<DTOs.ComercialDTO>>(json);
        }

        public async Task<DTOs.ComercialDTO> GetByIdAsync(int id)
        {
            var response = await client.GetAsync($"api/comerciales/{id}");
            
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<DTOs.ComercialDTO>(json);
        }
    }
}
