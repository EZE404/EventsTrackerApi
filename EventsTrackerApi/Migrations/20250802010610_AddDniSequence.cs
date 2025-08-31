using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsTrackerApi.Migrations
{
    /// <inheritdoc />
    public partial class AddDniSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        CREATE TABLE IF NOT EXISTS `dnisequence` (
            `next_not_cached_value` bigint(21) NOT NULL,
            `minimum_value` bigint(21) NOT NULL,
            `maximum_value` bigint(21) NOT NULL,
            `start_value` bigint(21) NOT NULL COMMENT 'start value',
            `increment` bigint(21) NOT NULL COMMENT 'increment',
            `cache_size` bigint(21) unsigned NOT NULL,
            `cycle_option` tinyint(1) unsigned NOT NULL,
            `cycle_count` bigint(21) NOT NULL
        ) ENGINE=InnoDB AUTO_INCREMENT=1;
    ");

            // Insertar el valor inicial
            migrationBuilder.Sql(@"
        INSERT INTO `dnisequence` (
            `next_not_cached_value`, `minimum_value`, `maximum_value`,
            `start_value`, `increment`, `cache_size`, `cycle_option`, `cycle_count`
        ) VALUES (10000000, 10000000, 99999999, 10000000, 1, 10, 0, 0);
    ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
