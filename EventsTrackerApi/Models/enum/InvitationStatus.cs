namespace EventsTrackerApi.Models
{
    /// <summary>
    /// Define los posibles estados de respuesta a una invitación a un evento.
    /// </summary>
    public enum InvitationStatus
    {
        /// <summary>
        /// El invitado aún no ha respondido.
        /// </summary>
        SIN_RESPUESTA,

        /// <summary>
        /// El invitado ha aceptado la invitación.
        /// </summary>
        ACEPTADA,

        /// <summary>
        /// El invitado ha rechazado la invitación.
        /// </summary>
        RECHAZADA,

        /// <summary>
        /// El invitado ha indicado que podría asistir.
        /// </summary>
        TAL_VEZ
    }
}
