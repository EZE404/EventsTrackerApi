namespace EventsTrackerApi.DTOs.Favorites
{
    /// <summary>
    /// DTO para representar un evento marcado como favorito por un usuario.
    /// Se utiliza en respuestas del API para mostrar información sobre favoritos.
    /// </summary>
    public class FavoriteDto
    {
        public int EventId { get; set; }
        public int UserId { get; set; }
        public string CreatedAt { get; set; }
    }
}

