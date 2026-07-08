using System;
using EventService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventService.Infrastructure.Persistence.Migrations;

[DbContext(typeof(EventServiceDbContext))]
[Migration("20260708000000_ConvertEventTimeToEventDate")]
public partial class ConvertEventTimeToEventDate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "event_time",
            table: "events",
            newName: "event_date");

        migrationBuilder.RenameIndex(
            name: "ix_events_is_deleted_event_time",
            table: "events",
            newName: "ix_events_is_deleted_event_date");

        migrationBuilder.RenameIndex(
            name: "ix_events_tenant_id_is_deleted_event_time",
            table: "events",
            newName: "ix_events_tenant_id_is_deleted_event_date");

        // event_time carried a full timestamp; event_date keeps only the UTC
        // calendar day. Upcoming/Past filtering moves from "now" to "today" as
        // a result (see EventListQueryBuilder).
        migrationBuilder.Sql(
            "ALTER TABLE events ALTER COLUMN event_date TYPE date USING (event_date AT TIME ZONE 'UTC')::date;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "ALTER TABLE events ALTER COLUMN event_date TYPE timestamp with time zone USING (event_date::timestamp AT TIME ZONE 'UTC');");

        migrationBuilder.RenameIndex(
            name: "ix_events_tenant_id_is_deleted_event_date",
            table: "events",
            newName: "ix_events_tenant_id_is_deleted_event_time");

        migrationBuilder.RenameIndex(
            name: "ix_events_is_deleted_event_date",
            table: "events",
            newName: "ix_events_is_deleted_event_time");

        migrationBuilder.RenameColumn(
            name: "event_date",
            table: "events",
            newName: "event_time");
    }
}
