using System;
using EventService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventService.Infrastructure.Persistence.Migrations;

[DbContext(typeof(EventServiceDbContext))]
[Migration("20260609000000_AddEventTenantId")]
public partial class AddEventTenantId : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "tenant_id",
            table: "events",
            type: "uuid",
            nullable: false);

        migrationBuilder.CreateIndex(
            name: "ix_events_tenant_id_is_deleted_event_time",
            table: "events",
            columns: new[] { "tenant_id", "is_deleted", "event_time" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "ix_events_tenant_id_is_deleted_event_time",
            table: "events");

        migrationBuilder.DropColumn(
            name: "tenant_id",
            table: "events");
    }
}
