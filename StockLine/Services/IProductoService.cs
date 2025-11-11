using System.Collections.Generic;
using System.Threading.Tasks;
using WpfApp1.DTOs;

namespace WpfApp1.Services
{
    public interface IProductoService
    {
        Task<List<ProductoDto>> GetAllAsync(string query = null);
        Task<ProductoDto> GetByIdAsync(int id);
    }
}
