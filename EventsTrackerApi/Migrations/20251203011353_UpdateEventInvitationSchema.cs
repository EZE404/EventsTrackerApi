using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsTrackerApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEventInvitationSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventInvitations_Events_EventID",
                table: "EventInvitations");

            migrationBuilder.DropForeignKey(
                name: "FK_EventInvitations_Users_CreatorID",
                table: "EventInvitations");

            migrationBuilder.DropForeignKey(
                name: "FK_EventInvitations_Users_UserID",
                table: "EventInvitations");

            migrationBuilder.DropIndex(
                name: "IX_EventInvitations_EventID",
                table: "EventInvitations");

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "EventInvitations",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "EventID",
                table: "EventInvitations",
                newName: "EventId");

            migrationBuilder.RenameColumn(
                name: "CreatorID",
                table: "EventInvitations",
                newName: "CreatorId");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "EventInvitations",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_EventInvitations_UserID",
                table: "EventInvitations",
                newName: "IX_EventInvitations_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_EventInvitations_CreatorID",
                table: "EventInvitations",
                newName: "IX_EventInvitations_CreatorId");

            migrationBuilder.AlterColumn<string>(
                name: "ResponseStatus",
                table: "EventInvitations",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_EventInvitations_EventId_UserId",
                table: "EventInvitations",
                columns: new[] { "EventId", "UserId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EventInvitations_Events_EventId",
                table: "EventInvitations",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventInvitations_Users_CreatorId",
                table: "EventInvitations",
                column: "CreatorId",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventInvitations_Users_UserId",
                table: "EventInvitations",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventInvitations_Events_EventId",
                table: "EventInvitations");

            migrationBuilder.DropForeignKey(
                name: "FK_EventInvitations_Users_CreatorId",
                table: "EventInvitations");

            migrationBuilder.DropForeignKey(
                name: "FK_EventInvitations_Users_UserId",
                table: "EventInvitations");

            migrationBuilder.DropIndex(
                name: "IX_EventInvitations_EventId_UserId",
                table: "EventInvitations");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "EventInvitations",
                newName: "UserID");

            migrationBuilder.RenameColumn(
                name: "EventId",
                table: "EventInvitations",
                newName: "EventID");

            migrationBuilder.RenameColumn(
                name: "CreatorId",
                table: "EventInvitations",
                newName: "CreatorID");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "EventInvitations",
                newName: "ID");

            migrationBuilder.RenameIndex(
                name: "IX_EventInvitations_UserId",
                table: "EventInvitations",
                newName: "IX_EventInvitations_UserID");

            migrationBuilder.RenameIndex(
                name: "IX_EventInvitations_CreatorId",
                table: "EventInvitations",
                newName: "IX_EventInvitations_CreatorID");

            migrationBuilder.AlterColumn<string>(
                name: "ResponseStatus",
                table: "EventInvitations",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_EventInvitations_EventID",
                table: "EventInvitations",
                column: "EventID");

            migrationBuilder.AddForeignKey(
                name: "FK_EventInvitations_Events_EventID",
                table: "EventInvitations",
                column: "EventID",
                principalTable: "Events",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventInvitations_Users_CreatorID",
                table: "EventInvitations",
                column: "CreatorID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EventInvitations_Users_UserID",
                table: "EventInvitations",
                column: "UserID",
                principalTable: "Users",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
