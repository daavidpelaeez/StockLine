namespace WpfApp1.ViewModels
{
    public class AyuntamientoViewModel
    {
        public int AyuntamientoID { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public string CP { get; set; }
        public string Ciudad { get; set; }
        public string Provincia { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public int? ComercialID { get; set; }
        public string ComercialNombre { get; set; }
        public string InicialNombre { get; set; }
        public bool Activo { get; set; }
    }
}
