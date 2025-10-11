using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    public  class Usuario
    {
        public Usuario()
        {
        }

        public long ID { get; set; }
        public String nombre { get; set; }
        public String correo { get; set; }
        public String password { get; set; }


        
    }
}
