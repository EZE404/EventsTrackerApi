using EventsTrackerApi.DTOs.Invitations;

namespace EventsTrackerApi.Service.Interfaces;

/// <summary>
/// Service for generating HTML email templates.
/// </summary>
public interface IEmailTemplateService
{
    /// <summary>
    /// Generates the password recovery email HTML.
    /// </summary>
    /// <param name="verificationNumber">The verification code.</param>
    /// <returns>The HTML email body.</returns>
    string GeneratePasswordRecoveryEmail(string verificationNumber);

    /// <summary>
    /// Generates the user data change email HTML.
    /// </summary>
    /// <param name="userName">The user's name.</param>
    /// <param name="dni">The user's DNI.</param>
    /// <param name="randomPassword">The generated password.</returns>
    /// <returns>The HTML email body.</returns>
    string GenerateUserDataChangeEmail(string userName, string dni, string randomPassword);

    /// <summary>
    /// Generates the invitation email HTML.
    /// </summary>
    /// <param name="model">The invitation data model.</param>
    /// <returns>The HTML email body.</returns>
    string GenerateInvitationEmail(InvitationEmailModelDto model);
}
