using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WpfApp1.DTOs;
using WpfApp1.Models;
using System.Windows;

namespace WpfApp1.Services
{
    public interface IEnvioService
    {
        Task<List<EnvioDTO>> GetAllAsync();
        Task<EnvioDTO> GetByIdAsync(int id);
        Task<EnvioDTO> CreateAsync(CrearEnvioDTO envio);
        Task<bool> UpdateEstadoAsync(int id, string estado, int? usuarioModificadorId = null);
        Task<bool> DeleteAsync(int id);
    }

    public class EnvioService : IEnvioService
    {
        private static readonly HttpClient client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5200/")
        };

        public async Task<List<EnvioDTO>> GetAllAsync()
        {
            var response = await client.GetAsync("api/envios");
            
            if (!response.IsSuccessStatusCode)
                return new List<EnvioDTO>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<EnvioDTO>>(json);
        }

        public async Task<EnvioDTO> GetByIdAsync(int id)
        {
            var response = await client.GetAsync($"api/envios/{id}");
            
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<EnvioDTO>(json);
        }

        public async Task<EnvioDTO> CreateAsync(CrearEnvioDTO envio)
        {
            var json = JsonConvert.SerializeObject(envio);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/envios", content);
            
            if (!response.IsSuccessStatusCode)
                return null;

            var responseJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<EnvioDTO>(responseJson);
        }

        public async Task<bool> UpdateEstadoAsync(int id, string estado, int? usuarioModificadorId = null)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== ACTUALIZANDO ESTADO ===");
                System.Diagnostics.Debug.WriteLine($"EnvioID: {id}");
                System.Diagnostics.Debug.WriteLine($"Estado: '{estado}'");
                System.Diagnostics.Debug.WriteLine($"UsuarioModificadorID: {usuarioModificadorId}");
                
                var json = JsonConvert.SerializeObject(estado);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var url = $"api/envios/{id}/estado";
                if (usuarioModificadorId.HasValue)
                {
                    url += $"?usuarioModificadorId={usuarioModificadorId.Value}";
                }

                var request = new HttpRequestMessage(new HttpMethod("PATCH"), url)
                {
                    Content = content
                };

                var response = await client.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"ERROR: {error}");
                    
                    MessageBox.Show(
                        $"Error al actualizar estado:\n\n{error}\n\n" +
                        $"Estados validos: Pendiente, Preparado, Enviado",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    
                    return false;
                }
                
                System.Diagnostics.Debug.WriteLine("Estado actualizado correctamente");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EXCEPCION: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await client.DeleteAsync($"api/envios/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
