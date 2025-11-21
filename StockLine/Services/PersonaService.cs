using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WpfApp1.DTOs;
using System.Linq;
using System.Windows;

namespace WpfApp1.Services
{
    public class PersonaService : IPersonaService
    {
        private static readonly HttpClient client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5200/")
        };

        public async Task<List<UsuarioDTO>> GetAllAsync(string query = null)
        {
            try
            {
                var url = "api/usuarios" + (string.IsNullOrWhiteSpace(query) ? "" : query);
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode) return new List<UsuarioDTO>();
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<List<UsuarioDTO>>(json);
                // Filtra elementos nulos y asegura que Activo se mapea correctamente
                return (result ?? new List<UsuarioDTO>()).Where(u => u != null).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en GetAllAsync: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<UsuarioDTO>();
            }
        }

        public async Task<UsuarioDTO> LoginAsync(string email, string password)
        {
            var loginData = new { Email = email, Password = password };
            var json = JsonConvert.SerializeObject(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/auth/login", content);
            if (!response.IsSuccessStatusCode)
                return null;
            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<LoginResponse>(responseJson);
            return result.User;
        }

        public async Task<bool> CreateAsync(UsuarioDTO usuario)
        {
            // Usar el endpoint correcto para registro
            var json = JsonConvert.SerializeObject(usuario);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/Auth/register", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateAsync(UsuarioDTO usuario)
        {
            var json = JsonConvert.SerializeObject(usuario);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"api/usuarios/{usuario.UsuarioID}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var response = await client.DeleteAsync($"api/usuarios/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                var errorContent = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    throw new InvalidOperationException(
                        "El usuario no puede eliminarse porque tiene registros asociados.\n" +
                        "Puede tener movimientos de stock o envios asignados.\n\n" +
                        "Detalles: " + errorContent);
                }
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new InvalidOperationException("El usuario no existe o ya fue eliminado.");
                }
                if ((int)response.StatusCode >= 500)
                {
                    throw new InvalidOperationException(
                        "Error del servidor al intentar eliminar el usuario.\n" +
                        "Codigo: " + (int)response.StatusCode);
                }
                if (!string.IsNullOrEmpty(errorContent))
                {
                    throw new InvalidOperationException("Error: " + errorContent);
                }
                return false;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (HttpRequestException httpEx)
            {
                throw new InvalidOperationException(
                    "Error de conexion con el servidor.\n" +
                    "Verifica que la API este en ejecucion.\n\n" +
                    "Detalles: " + httpEx.Message, 
                    httpEx);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Error inesperado al eliminar el usuario.\n\n" +
                    "Detalles: " + ex.Message, 
                    ex);
            }
        }
    }
}
