using System;
using System.Collections.Generic;

namespace WpfApp1.DTOs
{
    public class MovimientoDto
    {
        public int MovimientoID { get; set; }
        public DateTime Fecha { get; set; }
        public int ProductoID { get; set; }
        public string ProductoNombre { get; set; } = "";
        public int Cantidad { get; set; }
        public string TipoMovimiento { get; set; } = ""; 
        public int UsuarioID { get; set; }
        public string UsuarioNombre { get; set; } = "";
        public string Observaciones { get; set; } = "";
        public int? StockAfter { get; set; }
        
        public string Tipo { get; set; } 
        public List<ProductoMovimientoDTO> Productos { get; set; } 
    }

    public class ProductoMovimientoDTO
    {
        public int ProductoID { get; set; }
        public string ProductoNombre { get; set; }
        public int Cantidad { get; set; }
    }
}
