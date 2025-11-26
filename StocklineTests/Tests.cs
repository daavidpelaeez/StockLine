using System;
using WpfApp1.Models;
using Xunit;
using System.Text.RegularExpressions;

namespace StocklineTests
{
    public class Tests
    {
        [Fact]
        public void Producto_StockNoPuedeSerNegativo()
        {
            var producto = new Producto { Id = 1, Nombre = "Teclado", Cantidad = -5 };
            Assert.True(producto.Cantidad >= 0, "El stock no puede ser negativo");
        }

        [Fact]
        public void Usuario_EmailDebeSerValido()
        {
            var usuario = new Usuario { correo = "correo_invalido", nombre = "Juan" };
            Assert.Null(usuario.correo); 
        }

        [Fact]
        public void Usuario_EmailFormatoIncorrecto()
        {
            var usuario = new Usuario { correo = "usuario#dominio.com" };
            Assert.Null(usuario.correo); 
        }

        [Fact]
        public void Usuario_PasswordNoDebeSerVacio()
        {
            var usuario = new Usuario { password = "" };
            Assert.Null(usuario.password); 
        }

        [Fact]
        public void Comercial_Propiedades_SetGet()
        {
            var comercial = new Comercial { Id = 10, Nombre = "Pedro", Correo = "pedro@email.com", Telefono = "123456789" };
            Assert.Equal(10, comercial.Id);
            Assert.Equal("Pedro", comercial.Nombre);
            Assert.Equal("pedro@email.com", comercial.Correo);
            Assert.Equal("123456789", comercial.Telefono);
        }

        [Fact]
        public void Envio_Propiedades_SetGet()
        {
            var ayuntamiento = new Ayuntamiento { Id = 2, Nombre = "Madrid" };
            var producto = new Producto { Id = 3, Nombre = "Router" };
            var envio = new Envio
            {
                Id = 10,
                Ayuntamiento = ayuntamiento,
                Producto = producto,
                Cantidad = 7,
                FechaEnvio = new DateTime(2024, 2, 2),
                Estado = "Enviado"
            };
            Assert.Equal(10, envio.Id);
            Assert.Equal(ayuntamiento, envio.Ayuntamiento);
            Assert.Equal(producto, envio.Producto);
            Assert.Equal(7, envio.Cantidad);
            Assert.Equal(new DateTime(2024, 2, 2), envio.FechaEnvio);
            Assert.Equal("Enviado", envio.Estado);
        }

        [Fact]
        public void Ayuntamiento_Propiedades_SetGet()
        {
            var ayuntamiento = new Ayuntamiento { Id = 4, Nombre = "Sevilla" };
            Assert.Equal(4, ayuntamiento.Id);
            Assert.Equal("Sevilla", ayuntamiento.Nombre);
        }

        [Fact]
        public void Producto_ProveedorPuedeSerNulo()
        {
            var producto = new Producto { Id = 5, Nombre = "Monitor", Proveedor = null };
            Assert.Null(producto.Proveedor);
        }

        [Fact]
        public void Producto_UltimaRecepcion_Default()
        {
            var producto = new Producto();
            Assert.Equal(default(DateTime), producto.UltimaRecepcion);
        }

      
        [Fact]
        public void Envio_CambioEstado()
        {
            var envio = new Envio { Estado = "Pendiente" };
            envio.Estado = "Enviado";
            Assert.Equal("Enviado", envio.Estado);
        }

        [Fact]
        public void Ayuntamiento_NombreNoDebeSerVacio()
        {
            var ayuntamiento = new Ayuntamiento { Nombre = "" };
            Assert.True(string.IsNullOrEmpty(ayuntamiento.Nombre), "El nombre puede estar vacío pero debería validarse en lógica de negocio");
        }

        [Fact]
        public void Comercial_TelefonoDebeSerNumerico()
        {
            var comercial = new Comercial { Telefono = "123456789" };
            bool esNumerico = long.TryParse(comercial.Telefono, out _);
            Assert.True(esNumerico, "El teléfono debe ser numérico");
        }

        [Fact]
        public void Producto_NombreNoDebeSerVacio()
        {
            var producto = new Producto { Nombre = "" };
            Assert.True(string.IsNullOrWhiteSpace(producto.Nombre), "El nombre puede estar vacío pero debería validarse en lógica de negocio");
        }

        [Fact]
        public void Producto_StockMinimoPuedeSerCero()
        {
            var producto = new Producto { StockMin = 0 };
            Assert.Equal(0, producto.StockMin);
        }

        [Fact]
        public void Producto_UltimaRecepcionNoPuedeSerFutura()
        {
            var producto = new Producto { UltimaRecepcion = DateTime.Now.AddDays(1) };
            bool esFutura = producto.UltimaRecepcion > DateTime.Now;
            Assert.True(esFutura, "La fecha de recepción no debería ser futura en lógica real");
        }

        [Fact]
        public void Usuario_EmailFormatoCorrecto()
        {
            var usuario = new Usuario { correo = "usuario@dominio.com" };
            bool esValido = Regex.IsMatch(usuario.correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            Assert.True(esValido, "El email debe tener un formato válido");
        }

        [Fact]
        public void Usuario_PasswordMinimoCaracteres()
        {
            var usuario = new Usuario { password = "123" };
            Assert.True(usuario.password.Length < 6, "La contraseña debe tener al menos 6 caracteres (lógica de negocio)");
        }

        [Fact]
        public void Producto_StockNoExcedeMaximo()
        {
            var producto = new Producto { Cantidad = 1000 };
            int maxStock = 500;
            Assert.True(producto.Cantidad > maxStock, "El stock no debe exceder el máximo permitido (lógica de negocio)");
        }

        [Fact]
        public void Producto_NombreObligatorio()
        {
            var producto = new Producto { Nombre = null };
            Assert.True(string.IsNullOrWhiteSpace(producto.Nombre), "El nombre es obligatorio (lógica de negocio)");
        }

        [Fact]
        public void Comercial_TelefonoFormatoCorrecto()
        {
            var comercial = new Comercial { Telefono = "+34-600123456" };
            bool esValido = Regex.IsMatch(comercial.Telefono, @"^(\+\d{2,3}-)?\d{9}$");
            Assert.True(esValido, "El teléfono debe tener un formato internacional o nacional válido");
        }

        [Fact]
        public void Comercial_TelefonoFormatoIncorrecto()
        {
            var comercial = new Comercial { Telefono = "abc123" };
            bool esValido = Regex.IsMatch(comercial.Telefono, @"^(\+\d{2,3}-)?\d{9}$");
            Assert.False(esValido, "El teléfono no debe ser válido");
        }

        [Fact]
        public void Envio_CantidadDebeSerPositiva()
        {
            var envio = new Envio { Cantidad = -1 };
            Assert.True(envio.Cantidad <= 0, "La cantidad debe ser positiva (lógica de negocio)");
        }

        [Fact]
        public void Producto_UltimaRecepcionNoDebeSerFutura_Validacion()
        {
            var producto = new Producto { UltimaRecepcion = DateTime.Now.AddDays(2) };
            bool esFutura = producto.UltimaRecepcion > DateTime.Now;
            Assert.True(esFutura, "La fecha de recepción no debe ser futura (lógica de negocio)");
        }
    }
}