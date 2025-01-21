using System.ComponentModel.DataAnnotations;

namespace EventsTrackerApi.Models;
public class VerificationRequest
	{
		[DataType(DataType.EmailAddress)]
		public required string Email { get; set; }
        
		public required string VerificationNumber { get; set; }
	}