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
    public class PersonaService : IPersonaService
    {
        private static readonly HttpClient client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:8080/api/usuarios")
        };

        public async Task<bool> CreateAsync(Usuario usuario)
        {
            var json = JsonConvert.SerializeObject(usuario);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await client.DeleteAsync($"/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<Usuario>> GetAllAsync()
        {
            var response = await client.GetAsync("");
            if (!response.IsSuccessStatusCode)
                return new List<Usuario>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Usuario>>(json);
        }

        public async Task<bool> UpdateAsync(Usuario usuario)
        {
            var json = JsonConvert.SerializeObject(usuario);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"/{usuario.ID}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> LoginAsync(string nombre, string password)
        {
            var loginData = new { nombre = nombre, password = password };
            var json = JsonConvert.SerializeObject(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("usuarios/login", content);

            return response.IsSuccessStatusCode; // true si 200 OK, false si no
        }
    }
}
