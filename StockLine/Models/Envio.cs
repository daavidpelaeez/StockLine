using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    public class Envio
    {
        public long Id { get; set; }
        public Ayuntamiento Ayuntamiento { get; set; }
        public Producto Producto { get; set; }
        public int Cantidad { get; set; }
        public DateTime FechaEnvio { get; set; }
        public string Estado { get; set; }
    }
}
