using EventService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventService.Infrastructure.Persistence.Migrations;

[DbContext(typeof(EventServiceDbContext))]
[Migration("20260706000000_AddEventLocationAndTimeZone")]
public partial class AddEventLocationAndTimeZone : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "location_venue_name",
            table: "events",
            type: "character varying(200)",
            maxLength: 200,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "location_address",
            table: "events",
            type: "character varying(500)",
            maxLength: 500,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "location_online_url",
            table: "events",
            type: "character varying(2048)",
            maxLength: 2048,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "location_notes",
            table: "events",
            type: "character varying(2000)",
            maxLength: 2000,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "time_zone_id",
            table: "events",
            type: "character varying(64)",
            maxLength: 64,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "location_venue_name",
            table: "events");

        migrationBuilder.DropColumn(
            name: "location_address",
            table: "events");

        migrationBuilder.DropColumn(
            name: "location_online_url",
            table: "events");

        migrationBuilder.DropColumn(
            name: "location_notes",
            table: "events");

        migrationBuilder.DropColumn(
            name: "time_zone_id",
            table: "events");
    }
}
