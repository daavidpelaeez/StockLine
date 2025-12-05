using System;

namespace WpfApp1.Models
{
    
    public class NotificacionDescartada
    {
        
        public string TipoNotificacion { get; set; }

        
        public DateTime FechaDescarte { get; set; }

        
        public int? IdentificadorInterno { get; set; }

        
        public int VecesDescartada { get; set; }
    }
}
