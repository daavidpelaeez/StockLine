using System;

namespace WpfApp1.Models
{
    /// <summary>
    /// Representa una notificación que ha sido descartada por el usuario
    /// </summary>
    public class NotificacionDescartada
    {
        /// <summary>
        /// Tipo de notificación (StockBajo, EnviosPendientes, etc.)
        /// </summary>
        public string TipoNotificacion { get; set; }

        /// <summary>
        /// Fecha en que fue descartada
        /// </summary>
        public DateTime FechaDescarte { get; set; }

        /// <summary>
        /// Identificador opcional para diferencias entre instancias del mismo tipo
        /// Por ejemplo, para saber exactamente cuántos productos tienen stock bajo
        /// </summary>
        public int? IdentificadorInterno { get; set; }

        /// <summary>
        /// Contador del número de veces que se ha descartado
        /// </summary>
        public int VecesDescartada { get; set; }
    }
}
