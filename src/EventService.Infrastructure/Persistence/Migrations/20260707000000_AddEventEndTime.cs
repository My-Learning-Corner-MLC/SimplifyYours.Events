using System;
using EventService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventService.Infrastructure.Persistence.Migrations;

[DbContext(typeof(EventServiceDbContext))]
[Migration("20260707000000_AddEventEndTime")]
public partial class AddEventEndTime : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTimeOffset>(
            name: "event_end_time",
            table: "events",
            type: "timestamp with time zone",
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "event_end_time",
            table: "events");
    }
}
