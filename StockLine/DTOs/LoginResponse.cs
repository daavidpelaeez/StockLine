using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.DTOs
{
    public class LoginResponse
    {
        public string Message { get; set; }
        public UsuarioDTO User { get; set; }
    }
}
