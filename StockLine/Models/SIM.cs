using System;
using WpfApp1.Models;

namespace StockLine.Models
{
    public class SIM
    {
        public int SIMID { get; set; }
        public string NumeroSIM { get; set; }
        public int? ProductoID { get; set; }
        public Producto Producto { get; set; }
        public string Ubicacion { get; set; }
        public string Estado { get; set; }
        public DateTime? FechaAsignacion { get; set; }
        // ProductoNombre eliminado, solo debe estar en el DTO
    }
}