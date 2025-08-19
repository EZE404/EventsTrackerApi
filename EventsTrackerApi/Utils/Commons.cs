using System.Security.Cryptography;
using EventsTrackerApi.Data;
using Microsoft.EntityFrameworkCore;

namespace EventsTrackerApi.Utils;

public class Commons
{
    public static string CreatePasswordHash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 10);
    }

    public static bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }

    public static string GenerateVerificationNumber()
    {
        return new Random().Next(100000, 999999).ToString();
    }

    public static int GeneratePayNumber()
    {
        return new Random().Next(100000, 999999);
    }

    public static async Task<int> GetNextDniAsync(AppDbContext dbContext)
    {
        var connection = dbContext.Database.GetDbConnection();
        await connection.OpenAsync();

        using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT NEXT VALUE FOR eventstracker.DniSequence";
            var result = await command.ExecuteScalarAsync();

            return Convert.ToInt32(result);
        }
    }

    public static string HtmlBodyEmailRecoveryPassword(string verificationNumber)
    {
        return $@"
        <html>
        <head>
            <style>
                .email-body {{
                    font-family: Arial, sans-serif;
                    line-height: 1.6;
                    background-color: #f4f4f4;
                    padding: 20px;
                }}
                .email-content {{
                    background-color: #ffffff;
                    padding: 20px;
                    border-radius: 8px;
                    margin: 20px auto;
                    max-width: 600px;
                    text-align: center;
                }}
                .verification-number {{
                    font-size: 24px;
                    font-weight: bold;
                    color: #4CAF50;
                    margin: 20px 0;
                }}
            </style>
        </head>
        <body>
            <div class='email-body'>
                <div class='email-content'>
                    <h2>Recuperación de Contraseña Events Tracker</h2>
                    <p>Hemos recibido una solicitud para restablecer la contraseña de tu cuenta. Por favor, usa el siguiente código de verificación para restablecer tu contraseña:</p>
                    <h3 class='verification-number'>{verificationNumber}</h3>
                    <p>Si no solicitaste restablecer tu contraseña, por favor ignora este correo electrónico. Tu cuenta sigue siendo segura y no se ha realizado ningún cambio.</p>
                    <p>Muchas Gracias!</p>
                </div>
            </div>
        </body>
        </html>
    ";
    }

    public static string HtmlBodyEmailUserDataChange(string userName, string dni, string randomPassword)
    {
        return $@"
        <html>
        <head>
            <meta charset='utf-8' />
            <style>
                body {{
                    font-family: 'Segoe UI', Arial, sans-serif;
                    background-color: #f4f4f4;
                    margin: 0;
                    padding: 20px;
                }}
                .email-container {{
                    max-width: 600px;
                    margin: auto;
                    background-color: #ffffff;
                    border-radius: 10px;
                    box-shadow: 0 4px 12px rgba(0,0,0,0.1);
                    overflow: hidden;
                }}
                .header {{
                    background-color: #2e7d32;
                    color: white;
                    padding: 20px;
                    font-size: 22px;
                    font-weight: bold;
                    text-align: center;
                }}
                .content {{
                    padding: 20px;
                    color: #333333;
                    font-size: 16px;
                    line-height: 1.6;
                }}
                .highlight {{
                    font-weight: bold;
                    color: #ff9800;
                }}
                .user-info {{
                    background-color: #f1f8e9;
                    padding: 12px;
                    border-radius: 6px;
                    margin: 15px 0;
                    font-size: 15px;
                    border-left: 5px solid #4caf50;
                }}
                .password-section {{
                    background-color: #e8f5e9;
                    padding: 15px;
                    font-size: 20px;
                    font-weight: bold;
                    color: #2e7d32;
                    border-radius: 8px;
                    border: 1px solid #c8e6c9;
                    margin: 20px 0;
                    letter-spacing: 1px;
                }}
                .footer {{
                    text-align: center;
                    padding: 15px;
                    font-size: 14px;
                    color: #777;
                    background-color: #fafafa;
                    border-top: 1px solid #eee;
                }}
            </style>
        </head>
        <body>
            <div class='email-container'>
                <div class='header'>
                    Actualización de Datos en <span class='highlight'>Events Tracker</span>
                </div>
                <div class='content'>
                    <p>Hola {userName},</p>
                    <p>Hemos actualizado tus datos en nuestro sistema.</p>
                    <div class='user-info'>
                        <strong>DN&#8203;I:</strong> {dni}
                    </div>
                    <p>Además, se ha generado una nueva contraseña para tu cuenta:</p>
                    <div class='password-section'>
                        {randomPassword}
                    </div>
                    <p>Te recomendamos cambiar esta contraseña lo antes posible.</p>
                </div>
                <div class='footer'>
                    Muchas gracias,<br/>
                    El equipo de <span class='highlight'>Events Tracker</span>
                </div>
            </div>
        </body>
        </html>
        ";
    }

    private const string DefaultAllowed =
            "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789!@#$%^&*?-_";

    public static string GeneratePassword(int length, string? allowedChars = null)
    {
        if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length));

        string pool = allowedChars ?? DefaultAllowed;
        if (string.IsNullOrEmpty(pool)) throw new ArgumentException("El set de caracteres no puede estar vacío.", nameof(allowedChars));

        var buffer = new char[length];
        for (int i = 0; i < length; i++)
        {
            int idx = RandomNumberGenerator.GetInt32(pool.Length);
            buffer[i] = pool[idx];
        }
        return new string(buffer);
    }
}
