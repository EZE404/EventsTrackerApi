using System.Net;
using EventsTrackerApi.DTOs.Invitations;
using EventsTrackerApi.Service.Interfaces;

namespace EventsTrackerApi.Service;

/// <summary>
/// Implementation of email template generation using localized strings.
/// </summary>
public class EmailTemplateService : IEmailTemplateService
{
    private readonly ILocalizationService _localization;

    public EmailTemplateService(ILocalizationService localization)
    {
        _localization = localization;
    }

    /// <inheritdoc />
    public string GeneratePasswordRecoveryEmail(string verificationNumber)
    {
        return $@"
<html>
<head>
    <meta charset='utf-8' />
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px; }}
        .email-container {{ max-width: 600px; margin: auto; background-color: #ffffff; border-radius: 10px; box-shadow: 0 4px 12px rgba(0,0,0,0.1); overflow: hidden; }}
        .header {{ background-color: #2e7d32; color: white; padding: 20px; font-size: 22px; font-weight: bold; text-align: center; }}
        .content {{ padding: 20px; color: #333333; font-size: 16px; line-height: 1.6; }}
        .highlight {{ font-weight: bold; color: #ff9800; }}
        .code-section {{ background-color: #e8f5e9; padding: 15px; font-size: 22px; font-weight: 800; color: #2e7d32; border-radius: 8px; border: 1px solid #c8e6c9; margin: 20px 0; letter-spacing: 3px; text-align: center; font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, 'Liberation Mono', 'Courier New', monospace; }}
        .note {{ background-color: #fffde7; border-left: 5px solid #ffb300; padding: 12px; border-radius: 6px; margin-top: 10px; font-size: 14px; color: #5f5f5f; }}
        .footer {{ text-align: center; padding: 15px; font-size: 14px; color: #777; background-color: #fafafa; border-top: 1px solid #eee; }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='header'>
            {_localization.GetString("PasswordRecoveryHeader")} <span class='highlight'>{_localization.GetString("AppName")}</span>
        </div>
        <div class='content'>
            <p>{_localization.GetString("PasswordRecoveryGreeting")}</p>
            <p>{_localization.GetString("PasswordRecoveryBody")}</p>
            <div class='code-section'>{verificationNumber}</div>
            <p>{_localization.GetString("PasswordRecoveryExpiry")}</p>
            <div class='note'>{_localization.GetString("PasswordRecoveryTip")}</div>
        </div>
        <div class='footer'>
            {_localization.GetString("PasswordRecoveryFooter")}<br/>{_localization.GetString("PasswordRecoveryTeam")}
        </div>
    </div>
</body>
</html>";
    }

    /// <inheritdoc />
    public string GenerateUserDataChangeEmail(string userName, string dni, string randomPassword)
    {
        return $@"
<html>
<head>
    <meta charset='utf-8' />
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px; }}
        .email-container {{ max-width: 600px; margin: auto; background-color: #ffffff; border-radius: 10px; box-shadow: 0 4px 12px rgba(0,0,0,0.1); overflow: hidden; }}
        .header {{ background-color: #2e7d32; color: white; padding: 20px; font-size: 22px; font-weight: bold; text-align: center; }}
        .content {{ padding: 20px; color: #333333; font-size: 16px; line-height: 1.6; }}
        .highlight {{ font-weight: bold; color: #ff9800; }}
        .user-info {{ background-color: #f1f8e9; padding: 12px; border-radius: 6px; margin: 15px 0; font-size: 15px; border-left: 5px solid #4caf50; }}
        .password-section {{ background-color: #e8f5e9; padding: 15px; font-size: 20px; font-weight: bold; color: #2e7d32; border-radius: 8px; border: 1px solid #c8e6c9; margin: 20px 0; letter-spacing: 1px; }}
        .footer {{ text-align: center; padding: 15px; font-size: 14px; color: #777; background-color: #fafafa; border-top: 1px solid #eee; }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='header'>
            {_localization.GetString("UserDataChangeHeader")}
        </div>
        <div class='content'>
            <p>{_localization.GetFormattedString("DefaultGreeting")} {Html(userName)},</p>
            <p>{_localization.GetString("UserDataChangeBody")}</p>
            <div class='user-info'>
                <strong>{_localization.GetString("UserDataChangeDniLabel")}:</strong> {dni}
            </div>
            <p>{_localization.GetString("UserDataChangePasswordGenerated")}</p>
            <div class='password-section'>
                {randomPassword}
            </div>
            <p>{_localization.GetString("UserDataChangePasswordAdvice")}</p>
        </div>
        <div class='footer'>
            {_localization.GetString("UserDataChangeFooter")}<br/>
            {_localization.GetString("PasswordRecoveryTeam")}
        </div>
    </div>
</body>
</html>";
    }

    /// <inheritdoc />
    public string GenerateInvitationEmail(InvitationEmailModelDto model)
    {
        var receiverName = string.IsNullOrWhiteSpace(model.ReceiverName) 
            ? _localization.GetString("InvitationDefaultReceiver") 
            : Html(model.ReceiverName);
        var senderName = string.IsNullOrWhiteSpace(model.SenderName) 
            ? _localization.GetString("InvitationDefaultSender") 
            : Html(model.SenderName);
        var eventName = string.IsNullOrWhiteSpace(model.EventName) 
            ? _localization.GetString("InvitationDefaultEvent") 
            : Html(model.EventName);

        var hasDate = model.EventDate.HasValue;
        var hasLoc = !string.IsNullOrWhiteSpace(model.EventLocation);

        var dateText = hasDate ? model.EventDate!.Value.ToString("dd/MM/yyyy HH:mm") : null;
        var locationText = hasLoc ? Html(model.EventLocation!) : null;

        return $@"
<html>
<head>
    <meta charset='utf-8' />
    <meta name='viewport' content='width=device-width, initial-scale=1.0' />
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px; }}
        .email-container {{ max-width: 600px; margin: auto; background-color: #ffffff; border-radius: 10px; box-shadow: 0 4px 12px rgba(0,0,0,0.1); overflow: hidden; }}
        .header {{ background-color: #2e7d32; color: white; padding: 20px; font-size: 22px; font-weight: bold; text-align: center; }}
        .content {{ padding: 20px; color: #333333; font-size: 16px; line-height: 1.6; }}
        .highlight {{ font-weight: bold; color: #ff9800; }}
        .chip {{ display: inline-block; background: #e8f5e9; border: 1px solid #c8e6c9; padding: 10px 14px; border-radius: 999px; font-weight: 800; color: #2e7d32; margin: 10px 0; }}
        .user-info {{ background-color: #f1f8e9; padding: 12px; border-radius: 6px; margin: 15px 0; font-size: 15px; border-left: 5px solid #4caf50; }}
        .note {{ background-color: #fffde7; border-left: 5px solid #ffb300; padding: 12px; border-radius: 6px; margin-top: 10px; font-size: 14px; color: #5f5f5f; }}
        .footer {{ text-align: center; padding: 15px; font-size: 14px; color: #777; background-color: #fafafa; border-top: 1px solid #eee; }}
        .muted {{ color: #777; font-size: 13px; }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='header'>
            {_localization.GetString("InvitationHeader")} • <span class='highlight'>{_localization.GetString("AppName")}</span>
        </div>
        <div class='content'>
            <p>{receiverName}, {_localization.GetString("InvitationGreeting")}</p>
            <p><b>{senderName}</b> {_localization.GetString("InvitationInvitedBy")}</p>
            <div class='chip'>{eventName}</div>
            <div class='user-info'>
                <div><b>{_localization.GetString("InvitationLabel")}:</b> #{model.InvitationId}</div>
                {(hasDate ? $"<div><b>{_localization.GetString("InvitationDateLabel")}:</b> {Html(dateText!)}</div>" : "")}
                {(hasLoc ? $"<div><b>{_localization.GetString("InvitationLocationLabel")}:</b> {locationText}</div>" : "")}
            </div>
            <p>{_localization.GetString("InvitationAction")}</p>
            <div><span class='muted'>{_localization.GetString("InvitationInstructions")}</span></div>
            <div class='note'>{_localization.GetString("InvitationTip")}</div>
        </div>
        <div class='footer'>
            {_localization.GetString("PasswordRecoveryFooter")}<br/>
            <b>{_localization.GetString("AppName")}</b>
        </div>
    </div>
</body>
</html>";
    }

    private static string Html(string value) => WebUtility.HtmlEncode(value);
}
