using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WpfApp1.DTOs;

namespace WpfApp1.Services
{
    public interface IMovimientoService
    {
        Task<bool> CrearMovimientoAsync(MovimientoDto movimiento);
    }

    public class MovimientoService : IMovimientoService
    {
        private static readonly HttpClient client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5200/")
        };

        public async Task<bool> CrearMovimientoAsync(MovimientoDto movimiento)
        {
            var json = JsonConvert.SerializeObject(movimiento);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/movimientosstock", content);
            return response.IsSuccessStatusCode;
        }
    }
}
