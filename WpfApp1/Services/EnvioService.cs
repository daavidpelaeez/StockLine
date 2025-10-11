using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WpfApp1.Models;

namespace WpfApp1.Services
{
   public class EnvioService
    {
        private readonly HttpClient _httpClient;


        public EnvioService()
        {
            _httpClient = new HttpClient

            {
                BaseAddress = new Uri("http://localhost:8080/api/") 
            };
        }


        public async Task<List<Envio>> GetEnviosPendientesAsync()
        {
            var response = await _httpClient.GetAsync("envios/pendientes");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            // Deserializo a objeto
            return JsonConvert.DeserializeObject<List<Envio>>(json);
        }


        public async Task FinalizarEnvioAsync(long envioId)
        {
            var content = new StringContent("", Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"envios/{envioId}/finalizar", content);
            response.EnsureSuccessStatusCode();
        }
    }
}
