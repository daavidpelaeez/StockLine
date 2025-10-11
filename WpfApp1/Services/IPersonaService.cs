using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Models;

namespace WpfApp1.Services
{
    public interface IPersonaService
    {
        Task<List<Usuario>> GetAllAsync();
        Task<bool> CreateAsync(Usuario usuario);
        Task<bool> UpdateAsync(Usuario usuario);
        Task<bool> DeleteAsync(int id);
        Task<bool> LoginAsync(string username, string password);
    }
}
