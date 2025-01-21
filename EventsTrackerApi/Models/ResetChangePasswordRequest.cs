using System.ComponentModel.DataAnnotations;


namespace EventsTrackerApi.Models
{   public class ResetChangePasswordRequest {
        public required string Email { get; set; }
        
        [DataType(DataType.Password)]
        public required string NewPassword { get; set; }
        public required string VerificationNumber { get; set; }
    }
}
