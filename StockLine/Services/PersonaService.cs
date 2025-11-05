using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WpfApp1.DTOs;
using WpfApp1.Models;

namespace WpfApp1.Services
{
    public class PersonaService : IPersonaService
    {
        private static readonly HttpClient client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5200/") 
        };

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
            var json = JsonConvert.SerializeObject(usuario);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/usuarios", content);
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
                
                // Verificar si la respuesta es exitosa
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                
                // Leer el contenido del error
                var errorContent = await response.Content.ReadAsStringAsync();
                
                // Manejar código 409 Conflict (usuario con referencias)
                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    throw new InvalidOperationException(
                        "El usuario no puede eliminarse porque tiene registros asociados.\n" +
                        "Puede tener movimientos de stock o envios asignados.\n\n" +
                        "Detalles: " + errorContent);
                }
                
                // Manejar código 404 Not Found
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new InvalidOperationException("El usuario no existe o ya fue eliminado.");
                }
                
                // Otros errores del servidor (500, etc.)
                if ((int)response.StatusCode >= 500)
                {
                    throw new InvalidOperationException(
                        "Error del servidor al intentar eliminar el usuario.\n" +
                        "Codigo: " + (int)response.StatusCode);
                }
                
                // Error genérico con contenido
                if (!string.IsNullOrEmpty(errorContent))
                {
                    throw new InvalidOperationException("Error: " + errorContent);
                }
                
                return false;
            }
            catch (InvalidOperationException)
            {
                // Re-lanzar excepciones de negocio para que sean manejadas por la UI
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

        public async Task<List<UsuarioDTO>> GetAllAsync()
        {
            var response = await client.GetAsync("api/usuarios");
            if (!response.IsSuccessStatusCode) return new List<UsuarioDTO>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<UsuarioDTO>>(json);
        }
    }
}
