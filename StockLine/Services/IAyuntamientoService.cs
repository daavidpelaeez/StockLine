using System.Collections.Generic;
using System.Threading.Tasks;
using WpfApp1.DTOs;

namespace WpfApp1.Services
{
    public interface IAyuntamientoService
    {
        Task<List<AyuntamientoDTO>> GetAllAsync(string query = null);
        Task<AyuntamientoDTO> GetByIdAsync(int id);
        Task<bool> CreateAsync(AyuntamientoDTO ayuntamiento);
        Task<bool> UpdateAsync(AyuntamientoDTO ayuntamiento);
        Task<bool> DeleteAsync(int id);
    }
}
