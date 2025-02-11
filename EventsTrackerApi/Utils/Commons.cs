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
            box-shadow: 0 0 10px rgba(0,0,0,0.1);
        }}
        .user-info {{
            font-size: 16px;
            margin: 10px 0;
        }}
        .password-section {{
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
            <h2>Actualización de Datos en Events Tracker</h2>
            <p>Hola {userName},</p>
            <p>Hemos actualizado tus datos en nuestro sistema.</p>
            <div class='user-info'>
                <p><strong>DNI:</strong> {dni}</p>
            </div>
            <p>Además, se ha generado una nueva contraseña para tu cuenta:</p>
            <div class='password-section'>
                {randomPassword}
            </div>
            <p>Te recomendamos cambiar esta contraseña lo antes posible.</p>
            <p>Muchas gracias,<br/>El equipo de Events Tracker</p>
        </div>
    </div>
</body>
</html>
";
}

}
