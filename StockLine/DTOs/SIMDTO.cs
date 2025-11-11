using System;

namespace WpfApp1.DTOs
{
    public class SIMDTO
    {
        public int SIMID { get; set; }
        public string NumeroSIM { get; set; }
        public int? ProductoID { get; set; }
        public string ProductoNombre { get; set; }
        public string Ubicacion { get; set; } // Puede ser null o vacío si no aplica
        public string Estado { get; set; }
        public DateTime? FechaAsignacion { get; set; } // Nueva propiedad para lógica de ubicación
    }
}
