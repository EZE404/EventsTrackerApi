using EventsTrackerApi.Data;
using Microsoft.EntityFrameworkCore;
using MimeKit;

namespace EventsTrackerApi.Utils;
public class SenderEmail
{
    public static async Task<int> GetNextDniAsync(AppDbContext dbContext)
    {
        // Obtiene la conexión subyacente del contexto
        var connection = dbContext.Database.GetDbConnection();
        await connection.OpenAsync();

        using (var command = connection.CreateCommand())
        {
            // Ejecuta la consulta para obtener el siguiente valor de la secuencia.
            command.CommandText = "SELECT NEXT VALUE FOR eventstracker.DniSequence";
            var result = await command.ExecuteScalarAsync();

            // Convierte el resultado a entero
            return Convert.ToInt32(result);
        }
    }

    public static async Task SendResetEmail(EmailOptions mailOptions, IConfiguration configuration)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("Nombre del Remitente", mailOptions.From));
        emailMessage.To.Add(new MailboxAddress("Nombre del Destinatario", mailOptions.To));
        emailMessage.Subject = mailOptions.Subject;
        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = mailOptions.Body };

        using var client = new MailKit.Net.Smtp.SmtpClient();
        await client.ConnectAsync(configuration["EmailSettings:SmtpServer"], int.Parse(configuration["EmailSettings:Port"] ?? throw new InvalidOperationException()), false);
        await client.AuthenticateAsync(configuration["EmailSettings:SenderEmail"], configuration["EmailSettings:Password"]);
        await client.SendAsync(emailMessage);
        await client.DisconnectAsync(true);
    }
}
