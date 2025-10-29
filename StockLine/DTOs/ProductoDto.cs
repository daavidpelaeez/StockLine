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
    }
}
