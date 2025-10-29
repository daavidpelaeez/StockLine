using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.DTOs;
using WpfApp1.Models;

namespace WpfApp1.Services
{
    public interface IPersonaService
    {
        Task<UsuarioDTO> LoginAsync(string email, string password);
        Task<bool> CreateAsync(UsuarioDTO usuario);
        Task<bool> UpdateAsync(UsuarioDTO usuario);
        Task<bool> DeleteAsync(int id);
        Task<List<UsuarioDTO>> GetAllAsync();
    }
}
