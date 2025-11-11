using System.Collections.Generic;
using System.Threading.Tasks;
using WpfApp1.DTOs;

namespace WpfApp1.Services
{
    public interface ICategoriaService
    {
        Task<List<CategoriaDto>> GetAllAsync(string query = null);
        Task<CategoriaDto> GetByIdAsync(int id);
        Task<CategoriaDto> CreateAsync(CategoriaDto categoria);
        Task<bool> UpdateAsync(CategoriaDto categoria);
        Task<bool> DeleteAsync(int id);
        Task<int> GetProductosCountByCategoriaAsync(int categoriaId);
    }
}