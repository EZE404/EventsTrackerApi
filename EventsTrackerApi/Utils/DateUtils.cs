using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace EventsTrackerApi.Utils
{
    /// <summary>
    /// Clase de utilidad para manejar conversiones de fecha y hora,
    /// especialmente entre UTC y la hora local de Argentina.
    /// </summary>
    public static class DateUtils
    {
        private const string UtcFormat = "yyyy-MM-ddTHH:mm:ssZ";
        private static readonly TimeZoneInfo ArgentinaTimeZone = GetArgentinaTimeZone();

        /// <summary>
        /// Convierte un objeto DateTime (que se asume está en hora de Argentina) 
        /// a un string en formato ISO 8601 UTC.
        /// </summary>
        /// <param name="argentinaDateTime">La fecha y hora en Argentina.</param>
        /// <returns>Un string formateado en UTC (ej. "2024-05-21T14:35:10Z").</returns>
        public static string ToUtcString(DateTime argentinaDateTime)
        {
            // Convierte la hora de la zona horaria de Argentina a UTC.
            var utcTime = TimeZoneInfo.ConvertTimeToUtc(argentinaDateTime, ArgentinaTimeZone);
            return utcTime.ToString(UtcFormat, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Sobrecarga para manejar objetos DateTime nulables.
        /// </summary>
        public static string? ToUtcString(DateTime? argentinaDateTime)
        {
            return argentinaDateTime.HasValue ? ToUtcString(argentinaDateTime.Value) : null;
        }

        /// <summary>
        /// Devuelve la fecha y hora actual en la zona horaria de Argentina.
        /// Ideal para registrar marcas de tiempo en la lógica de negocio.
        /// </summary>
        /// <returns>Un objeto DateTime representando la hora actual en Argentina.</returns>
        public static DateTime NowInArgentina()
        {
            // Obtiene la hora UTC actual y la convierte a la zona horaria de Argentina.
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ArgentinaTimeZone);
        }

        /// <summary>
        /// Determina y devuelve la zona horaria de Argentina, manejando las diferencias
        /// de nomenclatura entre sistemas operativos Windows y Linux/macOS.
        /// </summary>
        private static TimeZoneInfo GetArgentinaTimeZone()
        {
            try
            {
                // El identificador estándar para la zona horaria de Argentina varía según el SO.
                string timeZoneId = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? "Argentina Standard Time"
                    : "America/Argentina/Buenos_Aires";
                
                return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException ex)
            {
                // Fallback en caso de que la zona horaria no se encuentre.
                // Esto podría ocurrir en un entorno de contenedor mal configurado.
                throw new InvalidOperationException("No se pudo encontrar la zona horaria de Argentina en el sistema.", ex);
            }
        }
    }
}
