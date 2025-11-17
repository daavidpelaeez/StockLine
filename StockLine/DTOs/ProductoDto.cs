using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.DTOs
{
    public class ProductoDto
    {
        public int ProductoID { get; set; }
        public string Nombre { get; set; } = "";          
        public string Descripcion { get; set; } = "";     
        public int Stock { get; set; }      
        public string Foto { get; set; } = "default.png";
        public int? CategoriaID { get; set; }             
        public string CategoriaNombre { get; set; } = ""; 
        
        // Propiedad calculada para determinar si el stock es critico
        public bool EsCritico => Stock < 10;

        public bool Activo { get; set; }

        // Propiedad calculada para la URL completa de la foto, forzando recarga con un GUID
        public string FotoUrl => $"http://localhost:5200/api/Productos/photo/{ProductoID}?v={Guid.NewGuid()}";
    }
}
