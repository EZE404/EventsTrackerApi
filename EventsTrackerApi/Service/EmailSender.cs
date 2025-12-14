using EventsTrackerApi.Data;
using EventsTrackerApi.Models;
using EventsTrackerApi.Utils;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using EventsTrackerApi.DTOs.Invitations;

namespace EventsTrackerApi.Service;

public class EmailSender : IEmailSender
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IOptions<EmailSettings> options, ILogger<EmailSender> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(EmailOptions mailOptions)
    {
        var message = new MimeMessage();
        var fromName = mailOptions.FromName ?? _settings.SenderName;
        var fromEmail = string.IsNullOrWhiteSpace(mailOptions.From) ? _settings.SenderEmail : mailOptions.From;

        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(new MailboxAddress(mailOptions.ToName ?? mailOptions.To, mailOptions.To));
        message.Subject = mailOptions.Subject;

        var builder = new BodyBuilder
        {
            HtmlBody = mailOptions.Body,
            TextBody = mailOptions.TextBody ?? "Para ver este mensaje, abre la versión HTML."
        };
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            var secure = _settings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
            await client.ConnectAsync(_settings.SmtpServer, _settings.Port, secure);
            await client.AuthenticateAsync(_settings.SenderEmail, _settings.Password);
            await client.SendAsync(message);
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }

    public Task SendPasswordRecoveryAsync(string to, string resetToken)
        => SendAsync(new EmailOptions
        {
            To = to,
            Subject = "Recuperación de Contraseña",
            Body = Commons.HtmlBodyEmailRecoveryPassword(resetToken)
        });

    public Task SendUserDataChangeAsync(string to, string firstName, string dni, string plainPassword)
        => SendAsync(new EmailOptions
        {
            To = to,
            Subject = "Actualizar los datos del usuario",
            Body = Commons.HtmlBodyEmailUserDataChange(firstName, dni, plainPassword)
        });

    public Task SendEventInvitationAsync(InvitationEmailModelDto invitationEmailModelDto)
    => SendAsync(new EmailOptions
    {
        To = invitationEmailModelDto.To,
        Subject = $"Te invitaron al evento: {invitationEmailModelDto.EventName}",
        Body = Commons.HtmlBodyInvitationEmail(invitationEmailModelDto)
    });
}
