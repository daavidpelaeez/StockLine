using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using WpfApp1.DTOs;

namespace WpfApp1.Services
{
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

        public async Task<List<AyuntamientoDTO>> GetAllAsync(string query = null)
        {
            try
            {
                var url = "api/ayuntamientos" + (string.IsNullOrWhiteSpace(query) ? "" : query);
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return new List<AyuntamientoDTO>();
                var json = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(json))
                    return new List<AyuntamientoDTO>();
                var ayuntamientos = JsonConvert.DeserializeObject<List<AyuntamientoDTO>>(json) ?? new List<AyuntamientoDTO>();
                var comerciales = await _comercialService.GetAllAsync();
                var comercialesDict = comerciales?.ToDictionary(c => c.ComercialID, c => c.Nombre + " " + c.Apellidos) 
                                      ?? new Dictionary<int, string>();
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
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
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
                if (id <= 0)
                {
                    throw new ArgumentException("El ID del ayuntamiento no es válido.", nameof(id));
                }
                response = await client.DeleteAsync($"api/ayuntamientos/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                try
                {
                    errorContent = await response.Content.ReadAsStringAsync();
                }
                catch
                {
                    errorContent = "No se pudo leer el detalle del error.";
                }
                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.NotFound:
                        throw new InvalidOperationException(
                            "El ayuntamiento no existe o ya fue eliminado.\n" +
                            "Por favor, actualiza la lista de ayuntamientos.");
                    case System.Net.HttpStatusCode.Conflict:
                        throw new InvalidOperationException(
                            "El ayuntamiento no puede eliminarse porque tiene registros asociados.\n" +
                            "Puede tener envíos o comerciales asignados.\n\n" +
                            "Detalles: " + (string.IsNullOrWhiteSpace(errorContent) ? "Sin detalles adicionales" : errorContent));
                    case System.Net.HttpStatusCode.BadRequest:
                        throw new InvalidOperationException(
                            "Solicitud inválida.\n\n" +
                            "Detalles: " + (string.IsNullOrWhiteSpace(errorContent) ? "Sin detalles adicionales" : errorContent));
                    case System.Net.HttpStatusCode.Unauthorized:
                    case System.Net.HttpStatusCode.Forbidden:
                        throw new InvalidOperationException(
                            "No tienes permisos para eliminar este ayuntamiento.\n" +
                            "Contacta con el administrador del sistema.");
                    case System.Net.HttpStatusCode.InternalServerError:
                    case System.Net.HttpStatusCode.BadGateway:
                    case System.Net.HttpStatusCode.ServiceUnavailable:
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
                if (response != null)
                {
                    response.Dispose();
                }
            }
        }
    }
}
