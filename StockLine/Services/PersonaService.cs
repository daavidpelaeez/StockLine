using System;
using System.Collections.Generic;
using System.Linq;
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
            BaseAddress = new Uri("http://localhost:5200/") // Cambiar según tu API
        };

        // LOGIN - devuelve UsuarioDTO si OK, null si falla
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

        // CREATE
        public async Task<bool> CreateAsync(UsuarioDTO usuario)
        {
            var json = JsonConvert.SerializeObject(usuario);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/usuarios", content);
            return response.IsSuccessStatusCode;
        }

        // UPDATE
        public async Task<bool> UpdateAsync(UsuarioDTO usuario)
        {
            var json = JsonConvert.SerializeObject(usuario);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"api/usuarios/{usuario.UsuarioID}", content);
            return response.IsSuccessStatusCode;
        }

        // DELETE
        public async Task<bool> DeleteAsync(int id)
        {
            var response = await client.DeleteAsync($"api/usuarios/{id}");
            return response.IsSuccessStatusCode;
        }

        // GET ALL
        public async Task<List<UsuarioDTO>> GetAllAsync()
        {
            var response = await client.GetAsync("api/usuarios");
            if (!response.IsSuccessStatusCode) return new List<UsuarioDTO>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<UsuarioDTO>>(json);
        }
    }
}
