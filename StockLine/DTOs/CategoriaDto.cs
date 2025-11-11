using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.DTOs
{
    public class CategoriaDto
    {
        public int CategoriaID { get; set; }
        public string Nombre { get; set; } = "";
        public bool Activo { get; set; }
    }
}
