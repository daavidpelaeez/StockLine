using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    public class Producto
    {
        public long Id { get; set; }
        public string Nombre { get; set; }
        public int Cantidad { get; set; }
        public string Categoria { get; set; }
        public string Ubicacion { get; set; }
        public int StockMin { get; set; }
        public string Proveedor { get; set; }
        public DateTime UltimaRecepcion { get; set; }
    }
}
