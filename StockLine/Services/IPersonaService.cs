using System.Collections.Generic;
using System.Threading.Tasks;
using WpfApp1.DTOs;

namespace WpfApp1.Services
{
    public interface IPersonaService
    {
        Task<List<UsuarioDTO>> GetAllAsync(string query = null);
        Task<UsuarioDTO> LoginAsync(string email, string password);
        Task<bool> CreateAsync(UsuarioDTO usuario);
        Task<bool> UpdateAsync(UsuarioDTO usuario);
        Task<bool> DeleteAsync(int id);
    }
}
