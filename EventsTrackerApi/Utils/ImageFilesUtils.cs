using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EventsTrackerApi.Utils
{
    /// <summary>
    /// Utilidad para guardar archivos de imagen en wwwroot en subcarpetas designadas
    /// devolviendo una URL relativa accesible mediante archivos estáticos.
    /// </summary>
    public static class ImageFilesUtils
    {
        private static readonly HashSet<string> DefaultAllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

        /// <summary>
        /// Guarda un archivo de imagen bajo wwwroot/{targetSubFolder} con un nombre único y devuelve su URL relativa.
        /// </summary>
        /// <param name="file">Archivo de imagen.</param>
        /// <param name="targetSubFolder">Subcarpeta relativa a wwwroot (por ejemplo: "uploads/flyers" o "uploads/avatars").</param>
        /// <param name="allowedExtensions">Conjunto de extensiones permitidas (incluyendo punto). Si es null, usa las por defecto.</param>
        /// <returns>URL relativa comenzando con "/" (por ejemplo: "/uploads/flyers/{guid}.jpg").</returns>
        /// <exception cref="ArgumentException">Si el archivo es inválido o la extensión no es permitida.</exception>
        public static async Task<string> SaveImageAsync(IFormFile file, string targetSubFolder, HashSet<string>? allowedExtensions = null)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("El archivo de imagen es requerido.");

            var ext = Path.GetExtension(file.FileName);
            var allowed = allowedExtensions ?? DefaultAllowedExtensions;
            if (string.IsNullOrWhiteSpace(ext) || !allowed.Contains(ext))
                throw new ArgumentException("Formato de imagen no soportado. Use: jpg, jpeg, png, webp o gif.");

            // Construir rutas
            var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            // Normalizar separadores de carpeta para Windows
            var safeSubFolder = targetSubFolder
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar)
                .TrimStart(Path.DirectorySeparatorChar);

            var targetFolderFs = Path.Combine(webRoot, safeSubFolder);
            Directory.CreateDirectory(targetFolderFs);

            var fileName = $"{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(targetFolderFs, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Construir URL relativa (usando separadores de URL "/")
            var relativeFolder = safeSubFolder.Replace(Path.DirectorySeparatorChar, '/');
            var relativeUrl = $"/{relativeFolder}/{fileName}";
            return relativeUrl;
        }

        /// <summary>
        /// Guarda una imagen de flyer bajo /uploads/flyers y devuelve su URL relativa.
        /// </summary>
        public static Task<string> SaveFlyerAsync(IFormFile file)
            => SaveImageAsync(file, "uploads/flyers");

        /// <summary>
        /// Guarda una imagen de avatar de usuario bajo /uploads/avatars y devuelve su URL relativa.
        /// </summary>
        public static Task<string> SaveUserAvatarAsync(IFormFile file)
            => SaveImageAsync(file, "uploads/avatars");

        /// <summary>
        /// Elimina en segundo plano una imagen bajo wwwroot dada su URL relativa.
        /// No bloquea la respuesta HTTP, ignora errores y previene path traversal.
        /// </summary>
        /// <param name="relativeUrl">Por ejemplo: "/uploads/avatars/xxx.jpg" o "uploads/flyers/yyy.png"</param>
        public static void DeleteImageInBackground(string? relativeUrl)
        {
            if (string.IsNullOrWhiteSpace(relativeUrl)) return;

            _ = Task.Run(async () =>
            {
                try
                {
                    var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                    var normalized = relativeUrl!.Replace('\\', '/').Trim();
                    if (normalized.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        // No se admite URL absoluta
                        return;
                    }

                    var relativePath = normalized.TrimStart('/');
                    var targetPath = Path.Combine(webRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));

                    var fullWebRoot = Path.GetFullPath(webRoot);
                    var fullTarget = Path.GetFullPath(targetPath);

                    // Prevenir path traversal
                    if (!fullTarget.StartsWith(fullWebRoot, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }

                    const int maxAttempts = 3;
                    for (int attempt = 1; attempt <= maxAttempts; attempt++)
                    {
                        try
                        {
                            if (File.Exists(fullTarget))
                            {
                                File.Delete(fullTarget);
                            }
                            break; // Éxito o archivo no existe
                        }
                        catch (IOException)
                        {
                            if (attempt == maxAttempts) break;
                            await Task.Delay(100);
                        }
                        catch (UnauthorizedAccessException)
                        {
                            // Sin permisos o archivo en uso sin posibilidad de borrar
                            break;
                        }
                    }
                }
                catch
                {
                    // Ignorar cualquier error no previsto
                }
            });
        }
    }
}
