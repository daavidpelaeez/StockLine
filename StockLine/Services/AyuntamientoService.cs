using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WpfApp1.DTOs;

namespace WpfApp1.Services
{
    public interface IAyuntamientoService
    {
        Task<List<AyuntamientoDTO>> GetAllAsync();
        Task<AyuntamientoDTO> GetByIdAsync(int id);
        Task<bool> CreateAsync(AyuntamientoDTO ayuntamiento);
        Task<bool> UpdateAsync(AyuntamientoDTO ayuntamiento);
        Task<bool> DeleteAsync(int id);
    }

    public class AyuntamientoService : IAyuntamientoService
    {
        private static readonly HttpClient client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5200/")
        };

        private readonly IComercialService _comercialService;

        public AyuntamientoService()
        {
            _comercialService = new ComercialService();
        }

        public async Task<List<AyuntamientoDTO>> GetAllAsync()
        {
            try
            {
                var response = await client.GetAsync("api/ayuntamientos");
                
                if (!response.IsSuccessStatusCode)
                    return new List<AyuntamientoDTO>();

                var json = await response.Content.ReadAsStringAsync();
                
                if (string.IsNullOrWhiteSpace(json))
                    return new List<AyuntamientoDTO>();
                
                var ayuntamientos = JsonConvert.DeserializeObject<List<AyuntamientoDTO>>(json) ?? new List<AyuntamientoDTO>();
                
                // Obtener todos los comerciales para mapear los nombres
                var comerciales = await _comercialService.GetAllAsync();
                var comercialesDict = comerciales?.ToDictionary(c => c.ComercialID, c => c.Nombre + " " + c.Apellidos) 
                                      ?? new Dictionary<int, string>();

                // Mapear el nombre del comercial si no viene en la respuesta
                foreach (var ayuntamiento in ayuntamientos)
                {
                    if (ayuntamiento.ComercialID.HasValue && 
                        string.IsNullOrWhiteSpace(ayuntamiento.ComercialNombre))
                    {
                        if (comercialesDict.TryGetValue(ayuntamiento.ComercialID.Value, out string nombreComercial))
                        {
                            ayuntamiento.ComercialNombre = nombreComercial;
                        }
                        else
                        {
                            ayuntamiento.ComercialNombre = "Sin asignar";
                        }
                    }
                    else if (!ayuntamiento.ComercialID.HasValue)
                    {
                        ayuntamiento.ComercialNombre = "Sin asignar";
                    }
                    
                    // Log para depuración
                    System.Diagnostics.Debug.WriteLine($"Ayuntamiento: {ayuntamiento.Nombre}, ComercialID: {ayuntamiento.ComercialID}, ComercialNombre: {ayuntamiento.ComercialNombre}");
                }
                
                return ayuntamientos;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en GetAllAsync: {ex.Message}");
                return new List<AyuntamientoDTO>();
            }
        }

        public async Task<AyuntamientoDTO> GetByIdAsync(int id)
        {
            try
            {
                var response = await client.GetAsync($"api/ayuntamientos/{id}");
                
                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                
                if (string.IsNullOrWhiteSpace(json))
                    return null;
                
                var ayuntamiento = JsonConvert.DeserializeObject<AyuntamientoDTO>(json);
                
                // Si no tiene el nombre del comercial, buscarlo
                if (ayuntamiento != null && 
                    ayuntamiento.ComercialID.HasValue && 
                    string.IsNullOrWhiteSpace(ayuntamiento.ComercialNombre))
                {
                    var comercial = await _comercialService.GetByIdAsync(ayuntamiento.ComercialID.Value);
                    if (comercial != null)
                    {
                        ayuntamiento.ComercialNombre = comercial.Nombre + " " + comercial.Apellidos;
                    }
                    else
                    {
                        ayuntamiento.ComercialNombre = "Sin asignar";
                    }
                }
                else if (ayuntamiento != null && !ayuntamiento.ComercialID.HasValue)
                {
                    ayuntamiento.ComercialNombre = "Sin asignar";
                }
                
                return ayuntamiento;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en GetByIdAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> CreateAsync(AyuntamientoDTO ayuntamiento)
        {
            try
            {
                if (ayuntamiento == null)
                    throw new ArgumentNullException(nameof(ayuntamiento));

                var json = JsonConvert.SerializeObject(ayuntamiento);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("api/ayuntamientos", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new InvalidOperationException("Error al crear el ayuntamiento: " + errorContent);
                }
                
                return response.IsSuccessStatusCode;
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (HttpRequestException httpEx)
            {
                throw new InvalidOperationException(
                    "Error de conexión con el servidor.\n" +
                    "Verifica que la API esté en ejecución.\n\n" +
                    "Detalles: " + httpEx.Message, 
                    httpEx);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Error inesperado al crear el ayuntamiento.\n\n" +
                    "Detalles: " + ex.Message, 
                    ex);
            }
        }

        public async Task<bool> UpdateAsync(AyuntamientoDTO ayuntamiento)
        {
            try
            {
                if (ayuntamiento == null)
                    throw new ArgumentNullException(nameof(ayuntamiento));

                var json = JsonConvert.SerializeObject(ayuntamiento);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"api/ayuntamientos/{ayuntamiento.AyuntamientoID}", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new InvalidOperationException("El ayuntamiento no existe.");
                    }
                    
                    throw new InvalidOperationException("Error al actualizar el ayuntamiento: " + errorContent);
                }
                
                return response.IsSuccessStatusCode;
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (HttpRequestException httpEx)
            {
                throw new InvalidOperationException(
                    "Error de conexión con el servidor.\n" +
                    "Verifica que la API esté en ejecución.\n\n" +
                    "Detalles: " + httpEx.Message, 
                    httpEx);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Error inesperado al actualizar el ayuntamiento.\n\n" +
                    "Detalles: " + ex.Message, 
                    ex);
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            HttpResponseMessage response = null;
            string errorContent = string.Empty;
            
            try
            {
                // Validar ID
                if (id <= 0)
                {
                    throw new ArgumentException("El ID del ayuntamiento no es válido.", nameof(id));
                }

                // Realizar la petición DELETE
                response = await client.DeleteAsync($"api/ayuntamientos/{id}");
                
                // Si fue exitoso, retornar true
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                
                // Leer el contenido del error de forma segura
                try
                {
                    errorContent = await response.Content.ReadAsStringAsync();
                }
                catch
                {
                    errorContent = "No se pudo leer el detalle del error.";
                }
                
                // Manejar diferentes códigos de estado HTTP
                switch (response.StatusCode)
                {
                    case HttpStatusCode.NotFound:
                        throw new InvalidOperationException(
                            "El ayuntamiento no existe o ya fue eliminado.\n" +
                            "Por favor, actualiza la lista de ayuntamientos.");
                    
                    case HttpStatusCode.Conflict:
                        throw new InvalidOperationException(
                            "El ayuntamiento no puede eliminarse porque tiene registros asociados.\n" +
                            "Puede tener envíos o comerciales asignados.\n\n" +
                            "Detalles: " + (string.IsNullOrWhiteSpace(errorContent) ? "Sin detalles adicionales" : errorContent));
                    
                    case HttpStatusCode.BadRequest:
                        throw new InvalidOperationException(
                            "Solicitud inválida.\n\n" +
                            "Detalles: " + (string.IsNullOrWhiteSpace(errorContent) ? "Sin detalles adicionales" : errorContent));
                    
                    case HttpStatusCode.Unauthorized:
                    case HttpStatusCode.Forbidden:
                        throw new InvalidOperationException(
                            "No tienes permisos para eliminar este ayuntamiento.\n" +
                            "Contacta con el administrador del sistema.");
                    
                    case HttpStatusCode.InternalServerError:
                    case HttpStatusCode.BadGateway:
                    case HttpStatusCode.ServiceUnavailable:
                        throw new InvalidOperationException(
                            "Error del servidor al intentar eliminar el ayuntamiento.\n" +
                            "Código: " + (int)response.StatusCode + " - " + response.ReasonPhrase + "\n\n" +
                            "Detalles: " + (string.IsNullOrWhiteSpace(errorContent) ? "Sin detalles adicionales" : errorContent));
                    
                    default:
                        throw new InvalidOperationException(
                            "Error al eliminar el ayuntamiento.\n" +
                            "Código HTTP: " + (int)response.StatusCode + " - " + response.ReasonPhrase + "\n\n" +
                            "Detalles: " + (string.IsNullOrWhiteSpace(errorContent) ? "Sin detalles adicionales" : errorContent));
                }
            }
            catch (ArgumentException)
            {
                // Re-lanzar excepciones de validación
                throw;
            }
            catch (InvalidOperationException)
            {
                // Re-lanzar excepciones de negocio para que sean manejadas por la UI
                throw;
            }
            catch (HttpRequestException httpEx)
            {
                throw new InvalidOperationException(
                    "Error de conexión con el servidor.\n" +
                    "Verifica que la API esté en ejecución en http://localhost:5200/\n\n" +
                    "Detalles técnicos:\n" + httpEx.Message, 
                    httpEx);
            }
            catch (TaskCanceledException)
            {
                throw new InvalidOperationException(
                    "La operación ha excedido el tiempo de espera.\n" +
                    "El servidor puede estar sobrecargado o no responde.\n\n" +
                    "Intenta nuevamente en unos momentos.");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "Error inesperado al eliminar el ayuntamiento.\n\n" +
                    "Tipo de error: " + ex.GetType().Name + "\n" +
                    "Mensaje: " + ex.Message + "\n\n" +
                    "Si el error persiste, contacta con el administrador del sistema.", 
                    ex);
            }
            finally
            {
                // Liberar recursos de forma segura
                if (response != null)
                {
                    response.Dispose();
                }
            }
        }
    }
}
