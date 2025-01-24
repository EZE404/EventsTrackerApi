using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsTrackerApi.Migrations
{
    /// <inheritdoc />
    public partial class AddDatabaseEventTracker1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Fecha_Actualizacion",
                table: "Users",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "Fecha_Creacion",
                table: "Users",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Fecha_Actualizacion",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Fecha_Creacion",
                table: "Users");
        }
    }
}
