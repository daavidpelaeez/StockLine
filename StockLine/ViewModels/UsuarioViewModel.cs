using System.Threading.Tasks;
using System.Windows.Media;
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

        public int UsuarioID { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string Email { get; set; }
        public int RoleID { get; set; }
        public bool Activo { get; set; }
        public string NombreCompleto { get; set; }
        public string InicialNombre { get; set; }
        public string RolNombre { get; set; }
        public Brush RolColor { get; set; }
        public string EstadoTexto => Activo ? "Activo" : "Inactivo";
        public Brush EstadoColor => Activo ?
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27AE60")) :
            new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C"));
    }
}
