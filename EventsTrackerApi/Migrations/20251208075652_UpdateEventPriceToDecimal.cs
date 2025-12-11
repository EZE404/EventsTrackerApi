using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsTrackerApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEventPriceToDecimal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "UserImages",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP(6)",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "Events",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "float");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "UserImages",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValueSql: "CURRENT_TIMESTAMP(6)");

            migrationBuilder.AlterColumn<float>(
                name: "Price",
                table: "Events",
                type: "float",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}
