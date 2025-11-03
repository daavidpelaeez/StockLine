using System;
using System.Collections.Generic;

namespace WpfApp1.DTOs
{
    public class EnvioDTO
    {
        public int EnvioID { get; set; }
        public int AyuntamientoID { get; set; }
        public string AyuntamientoNombre { get; set; }
        public int ComercialID { get; set; }
        public string ComercialNombre { get; set; }
        public DateTime FechaEnvio { get; set; }
        public string Estado { get; set; }
        public string NumeroReferencia { get; set; }
        public int? UsuarioModificadorID { get; set; }
        public string UsuarioModificadorNombre { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public List<EnvioDetalleDTO> Detalles { get; set; }
    }

    public class EnvioDetalleDTO
    {
        public int EnvioDetalleID { get; set; }
        public int ProductoID { get; set; }
        public string ProductoNombre { get; set; }
        public int Cantidad { get; set; }
        public int? SIMID { get; set; }
        public string SIMNumero { get; set; }
    }

    public class CrearEnvioDTO
    {
        public int AyuntamientoID { get; set; }
        public int ComercialID { get; set; }
        public DateTime FechaEnvio { get; set; }
        public string NumeroReferencia { get; set; }
        public int UsuarioID { get; set; }
        public List<CrearEnvioDetalleDTO> Productos { get; set; }
    }

    public class CrearEnvioDetalleDTO
    {
        public int ProductoID { get; set; }
        public int Cantidad { get; set; }
        public int? SIMID { get; set; }
    }
}
