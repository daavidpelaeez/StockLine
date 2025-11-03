using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WpfApp1.DTOs;

namespace WpfApp1.Services
{
    public interface IAyuntamientoService
    {
        Task<List<AyuntamientoDTO>> GetAllAsync();
        Task<AyuntamientoDTO> GetByIdAsync(int id);
    }

    public class AyuntamientoService : IAyuntamientoService
    {
        private static readonly HttpClient client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5200/")
        };

        public async Task<List<AyuntamientoDTO>> GetAllAsync()
        {
            var response = await client.GetAsync("api/ayuntamientos");
            
            if (!response.IsSuccessStatusCode)
                return new List<AyuntamientoDTO>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<AyuntamientoDTO>>(json);
        }

        public async Task<AyuntamientoDTO> GetByIdAsync(int id)
        {
            var response = await client.GetAsync($"api/ayuntamientos/{id}");
            
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<AyuntamientoDTO>(json);
        }
    }
}
