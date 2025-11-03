using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.DTOs;

namespace WpfApp1.Services
{
    public interface ICategoriaService
    {
        Task<List<CategoriaDto>> GetAllAsync();
        Task<CategoriaDto> GetByIdAsync(int id);
        Task<CategoriaDto> CreateAsync(CategoriaDto categoria);
        Task<bool> UpdateAsync(CategoriaDto categoria);
        Task<bool> DeleteAsync(int id);
        Task<int> GetProductosCountByCategoriaAsync(int categoriaId);
    }
}
