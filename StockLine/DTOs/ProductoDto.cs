using System;


namespace WpfApp1.DTOs
{
    public class ProductoDto
    {
        public int ProductoID { get; set; }
        public string Nombre { get; set; } = "";          
        public string Descripcion { get; set; } = "";     
        public int Stock { get; set; }      
        public string Foto { get; set; } = "default.png";
        public int? CategoriaID { get; set; }             
        public string CategoriaNombre { get; set; } = ""; 
        
       
        public bool EsCritico => Stock < 10;

        public bool Activo { get; set; }

       
        public string FotoUrl => $"http://localhost:5200/api/Productos/photo/{ProductoID}?v={Guid.NewGuid()}";

        public override string ToString()
        {
            return Nombre;
        }
    }
}
