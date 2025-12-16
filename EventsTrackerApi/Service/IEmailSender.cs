namespace EventsTrackerApi.Service;
using EventsTrackerApi.DTOs.Invitations;  

public interface IEmailSender
{
    Task SendAsync(EmailOptions options);
   
    Task SendPasswordRecoveryAsync(string to, string resetToken);
    Task SendUserDataChangeAsync(string to, string firstName, string dni, string plainPassword);
    Task SendEventInvitationAsync(InvitationEmailModelDto model);
}