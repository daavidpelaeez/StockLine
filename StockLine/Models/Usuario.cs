using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace WpfApp1.Models
{
    public class Usuario
    {
        public Usuario() { }

        public long ID { get; set; }
        public string nombre { get; set; }

        private string _correo;
        public string correo
        {
            get => _correo;
            set
            {
                if (!string.IsNullOrEmpty(value) && value.Contains("@") && value.Contains("."))
                    _correo = value;
                else
                    _correo = null;
            }
        }

        private string _password;
        public string password
        {
            get => _password;
            set
            {
                
                if (!string.IsNullOrWhiteSpace(value))
                    _password = value;
                else
                    _password = null;
            }
        }
    }
}
