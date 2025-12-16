using System.ComponentModel.DataAnnotations;
using EventsTrackerApi.DTOs;
using EventsTrackerApi.DTOs.Event;

namespace EventsTrackerApi.Utils
{
    public class DateValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is EventCreateFormDto createDto)
            {
                if (createDto.StartDateTime.Date < DateTime.Now.Date)
                {
                    return new ValidationResult("La fecha de inicio no puede ser anterior a la fecha actual.");
                }

                if (createDto.EndDateTime <= createDto.StartDateTime)
                {
                    return new ValidationResult("La fecha de finalización debe ser posterior a la fecha de inicio.");
                }
            }
            else if (value is EventUpdateDto updateDto)
            {
                if (updateDto.StartDateTime.Date < DateTime.Now.Date)
                {
                    return new ValidationResult("La fecha de inicio no puede ser anterior a la fecha actual.");
                }

                if (updateDto.EndDateTime <= updateDto.StartDateTime)
                {
                    return new ValidationResult("La fecha de finalización debe ser posterior a la fecha de inicio.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
