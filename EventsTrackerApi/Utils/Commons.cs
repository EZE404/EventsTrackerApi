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
                    <h2>Recuperación de Contraseña Inmobiliaria Cabrera</h2>
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

}
