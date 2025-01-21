using System.ComponentModel.DataAnnotations;

namespace EventsTrackerApi.Models;

public class ChangePasswordView {
    public required int Id { get; set; }
    
    [DataType(DataType.Password)]
    public required string CurrentPassword { get; set; }
    
    [DataType(DataType.Password)]
    public required string NewPassword { get; set; }
}
