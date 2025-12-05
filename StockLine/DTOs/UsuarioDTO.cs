

namespace WpfApp1.DTOs
{
    public class UsuarioDTO
    {
        public int UsuarioID { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string Email { get; set; }
        public int RoleID { get; set; }
        public bool Activo { get; set; } = true;
        public string Password { get; set; }
        public int? ComercialID { get; set; }
    }
}
