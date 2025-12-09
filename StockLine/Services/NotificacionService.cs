using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using WpfApp1.Models;

namespace WpfApp1.Services
{
    
    public class NotificacionService
    {
        private readonly string _rutaArchivo;
        private const string NOMBRE_ARCHIVO = "notificaciones_descartadas.json";
        private List<NotificacionDescartada> _notificacionesDescartadas;

        public NotificacionService()
        {
            
            string carpetaDatos = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "StockLine"
            );

           
            if (!Directory.Exists(carpetaDatos))
            {
                Directory.CreateDirectory(carpetaDatos);
            }

            _rutaArchivo = Path.Combine(carpetaDatos, NOMBRE_ARCHIVO);
            CargarNotificacionesDescartadas();
        }

       
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

       
        public void DescarrarNotificacion(string tipoNotificacion, int? identificador = null)
        {
            try
            {
                
                var notificacionExistente = _notificacionesDescartadas.FirstOrDefault(n =>
                    n.TipoNotificacion == tipoNotificacion &&
                    n.IdentificadorInterno == identificador
                );

                if (notificacionExistente != null)
                {
                   
                    notificacionExistente.VecesDescartada++;
                    notificacionExistente.FechaDescarte = DateTime.Now;
                }
                else
                {
                    
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

                
                var tiempoTranscurrido = DateTime.Now - notificacion.FechaDescarte;
                return tiempoTranscurrido.TotalHours < horasValidezDescarte;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al verificar notificación descartada: {ex.Message}");
                return false;
            }
        }

      
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

        
        public List<NotificacionDescartada> ObtenerNotificacionesDescartadas()
        {
            return _notificacionesDescartadas?.ToList() ?? new List<NotificacionDescartada>();
        }
    }
}
