using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;
using StockLine.Models;

namespace WpfApp1.Services
{
    public class GestionSIMsService
    {
        private static readonly HttpClient client = new HttpClient
        {
            BaseAddress = new System.Uri("http://localhost:5200/")
        };

        public async Task<int> GetSIMsDisponiblesAsync()
        {
            var response = await client.GetAsync("api/sims");
            if (!response.IsSuccessStatusCode)
                return 0;

            var json = await response.Content.ReadAsStringAsync();
            var sims = JsonConvert.DeserializeObject<List<SIM>>(json);

            // SIM disponible: ProductoID == null o ProductoID == 0
            return sims.Count(s => s.ProductoID == null || s.ProductoID == 0);
        }
    }
}
