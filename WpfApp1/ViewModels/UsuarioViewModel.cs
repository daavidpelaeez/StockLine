using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Services;

namespace WpfApp1.ViewModels
{
    public class UsuarioViewModel
    {
        private readonly IPersonaService _personaService;

        public UsuarioViewModel()
        {
            _personaService = new PersonaService(); 
        }

        public async Task CargarUsuarios()
        {
            var usuarios = await _personaService.GetAllAsync();
           
        }
    }
}
