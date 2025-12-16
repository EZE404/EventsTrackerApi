namespace EventsTrackerApi.DTOs.Posts
{
    /// <summary>
    /// DTO para el cuerpo de la solicitud de creación de un nuevo post.
    /// Contiene la información mínima necesaria para crear un comentario.
    /// </summary>
    public class CreatePostRequestDto
    {
        public int EventId { get; set; }
        public string Content { get; set; }
    }
}
