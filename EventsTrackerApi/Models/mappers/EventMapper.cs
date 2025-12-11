using EventsTrackerApi.DTOs;
using EventsTrackerApi.DTOs.Invitations;
using EventsTrackerApi.Models;

namespace EventsTrackerApi.Models.mappers
{
    public static class EventMapper
    {
        public static EventDTO? ToMapper(Event e)
        {
            if (e == null) return null;

            // Normalizado a 0-5 (partiendo de scores 1-10)
            var avg5 = e.RatingsCount == 0
                ? 0d
                : (double)e.RatingsSum / (2.0 * e.RatingsCount);

            return new EventDTO
            {
                Id = e.ID,
                Name = e.Name,
                Description = e.Description,
                Location = e.Location,
                StartDateTime = e.StartDateTime,
                EndDateTime = e.EndDateTime,
                Capacity = e.Capacity,
                CreatorID = e.CreatorID,
                Status = e.Status,
                FlyerUrl = e.FlyerUrl,
                Creator = UserMapper.ToMapper(e.Creator),
                Invitations = new List<object>(),
                Posts = new List<object>(),
                LocationId = e.LocationId,
                Tags = e.EventTags?
                        .Select(et => TagMapper.ToMapper(et.Tag))
                        .ToList()
                        ?? new List<TagDto>(),
                RatingCount = e.RatingsCount,
                RatingAverage = Math.Round(avg5, 2),
                RatingSum = e.RatingsSum,
                Price = e.Price
            };
        }

        public static Event ToModel(EventCreateDto dto, Location location, int creatorId)
        {
            if (dto == null || location == null) return null;

            return new Event
            {
                Name = dto.Name,
                Description = dto.Description,
                LocationId = location.Id,
                Location = location,
                StartDateTime = DateTime.SpecifyKind(dto.StartDateTime, DateTimeKind.Utc),
                EndDateTime = DateTime.SpecifyKind(dto.EndDateTime, DateTimeKind.Utc),
                Capacity = dto.Capacity,
                CreatorID = creatorId,
                Status = dto.Status,
                FlyerUrl = dto.FlyerUrl,
                Price = dto.Price // La asignación ahora es válida (decimal a decimal)
            };
        }

        // Sobrecarga para mapear directamente desde el DTO de formulario multipart
        public static Event ToModel(EventCreateFormDto form, Location location, int creatorId, string flyerUrl)
        {
            if (form == null || location == null) return null;

            return new Event
            {
                Name = form.Name,
                Description = form.Description,
                LocationId = location.Id,
                Location = location,
                StartDateTime = DateTime.SpecifyKind(form.StartDateTime, DateTimeKind.Utc),
                EndDateTime = DateTime.SpecifyKind(form.EndDateTime, DateTimeKind.Utc),
                Capacity = form.Capacity,
                CreatorID = creatorId,
                Status = form.Status,
                FlyerUrl = flyerUrl,
                Price = form.Price
            };
        }

        /// <summary>
        /// Convierte una entidad Event a un DTO de resumen (EventSummaryDto).
        /// </summary>
        /// <param name="ev">La entidad Event a convertir.</param>
        /// <returns>Un EventSummaryDto o null si la entrada es null.</returns>
        public static EventSummaryDto ToEventSummaryDto(Event ev)
        {
            if (ev == null)
            {
                return null;
            }

            return new EventSummaryDto
            {
                Id = ev.ID,
                Name = ev.Name
            };
        }
    }
}
