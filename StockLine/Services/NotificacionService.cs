using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using WpfApp1.Models;

namespace WpfApp1.Services
{
    /// <summary>
    /// Servicio para gestionar las notificaciones descartadas por el usuario
    /// Persiste el estado en un archivo JSON
    /// </summary>
    public class NotificacionService
    {
        private readonly string _rutaArchivo;
        private const string NOMBRE_ARCHIVO = "notificaciones_descartadas.json";
        private List<NotificacionDescartada> _notificacionesDescartadas;

        public NotificacionService()
        {
            // Guardar el archivo en la carpeta de datos de la aplicación
            string carpetaDatos = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "StockLine"
            );

            // Crear la carpeta si no existe
            if (!Directory.Exists(carpetaDatos))
            {
                Directory.CreateDirectory(carpetaDatos);
            }

            _rutaArchivo = Path.Combine(carpetaDatos, NOMBRE_ARCHIVO);
            CargarNotificacionesDescartadas();
        }

        /// <summary>
        /// Carga las notificaciones descartadas desde el archivo JSON
        /// </summary>
        private void CargarNotificacionesDescartadas()
        {
            try
            {
                if (File.Exists(_rutaArchivo))
                {
                    var json = File.ReadAllText(_rutaArchivo);
                    _notificacionesDescartadas = JsonConvert.DeserializeObject<List<NotificacionDescartada>>(json) 
                        ?? new List<NotificacionDescartada>();
                }
                else
                {
                    _notificacionesDescartadas = new List<NotificacionDescartada>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al cargar notificaciones descartadas: {ex.Message}");
                _notificacionesDescartadas = new List<NotificacionDescartada>();
            }
        }

        /// <summary>
        /// Guarda las notificaciones descartadas en el archivo JSON
        /// </summary>
        private void GuardarNotificacionesDescartadas()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_notificacionesDescartadas, Formatting.Indented);
                File.WriteAllText(_rutaArchivo, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al guardar notificaciones descartadas: {ex.Message}");
            }
        }

        /// <summary>
        /// Marca una notificación como descartada
        /// </summary>
        /// <param name="tipoNotificacion">Tipo de notificación (ej: "StockBajo")</param>
        /// <param name="identificador">ID opcional para diferencias entre instancias</param>
        public void DescarrarNotificacion(string tipoNotificacion, int? identificador = null)
        {
            try
            {
                // Buscar si ya existe una notificación descartada de este tipo
                var notificacionExistente = _notificacionesDescartadas.FirstOrDefault(n =>
                    n.TipoNotificacion == tipoNotificacion &&
                    n.IdentificadorInterno == identificador
                );

                if (notificacionExistente != null)
                {
                    // Si ya existe, incrementar el contador y actualizar fecha
                    notificacionExistente.VecesDescartada++;
                    notificacionExistente.FechaDescarte = DateTime.Now;
                }
                else
                {
                    // Si no existe, crear una nueva
                    var nuevaNotificacion = new NotificacionDescartada
                    {
                        TipoNotificacion = tipoNotificacion,
                        FechaDescarte = DateTime.Now,
                        IdentificadorInterno = identificador,
                        VecesDescartada = 1
                    };

                    _notificacionesDescartadas.Add(nuevaNotificacion);
                }

                GuardarNotificacionesDescartadas();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al descartar notificación: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica si una notificación ha sido descartada y aún está dentro del período de validez
        /// </summary>
        /// <param name="tipoNotificacion">Tipo de notificación</param>
        /// <param name="identificador">ID opcional para diferencias entre instancias</param>
        /// <param name="horasValidezDescarte">Horas durante las cuales la notificación descartada se mantiene válida (por defecto 24 horas)</param>
        /// <returns>True si fue descartada y aún está dentro del período válido</returns>
        public bool FueDescartada(string tipoNotificacion, int? identificador = null, int horasValidezDescarte = 24)
        {
            try
            {
                var notificacion = _notificacionesDescartadas.FirstOrDefault(n =>
                    n.TipoNotificacion == tipoNotificacion &&
                    n.IdentificadorInterno == identificador
                );

                if (notificacion == null)
                    return false;

                // Verificar si aún está dentro del período de validez
                var tiempoTranscurrido = DateTime.Now - notificacion.FechaDescarte;
                return tiempoTranscurrido.TotalHours < horasValidezDescarte;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al verificar notificación descartada: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Limpia todas las notificaciones descartadas
        /// </summary>
        public void LimpiarTodasLasNotificaciones()
        {
            try
            {
                _notificacionesDescartadas.Clear();
                GuardarNotificacionesDescartadas();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al limpiar notificaciones: {ex.Message}");
            }
        }

        /// <summary>
        /// Limpia las notificaciones descartadas que hayan expirado
        /// </summary>
        /// <param name="horasMaximas">Horas máximas de antigüedad para mantener</param>
        public void LimpiarNotificacionesExpiradas(int horasMaximas = 48)
        {
            try
            {
                var ahora = DateTime.Now;
                _notificacionesDescartadas = _notificacionesDescartadas
                    .Where(n => (ahora - n.FechaDescarte).TotalHours < horasMaximas)
                    .ToList();

                GuardarNotificacionesDescartadas();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al limpiar notificaciones expiradas: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene todas las notificaciones descartadas (para propósitos de debug)
        /// </summary>
        public List<NotificacionDescartada> ObtenerNotificacionesDescartadas()
        {
            return _notificacionesDescartadas?.ToList() ?? new List<NotificacionDescartada>();
        }
    }
}
