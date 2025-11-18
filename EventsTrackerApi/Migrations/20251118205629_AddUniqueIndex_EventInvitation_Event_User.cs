using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsTrackerApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndex_EventInvitation_Event_User : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Esto lo desactivo porque el índice no se puede borrar, por estar siendo usado para la clave foránea.
            /*migrationBuilder.DropIndex(
                name: "IX_EventInvitations_EventID",
                table: "EventInvitations");*/

            migrationBuilder.CreateIndex(
                name: "IX_EventInvitations_EventID_UserID",
                table: "EventInvitations",
                columns: new[] { "EventID", "UserID" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EventInvitations_EventID_UserID",
                table: "EventInvitations");

            /*migrationBuilder.CreateIndex(
                name: "IX_EventInvitations_EventID",
                table: "EventInvitations",
                column: "EventID");*/
        }
    }
}
