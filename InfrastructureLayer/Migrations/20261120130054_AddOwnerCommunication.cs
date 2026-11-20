using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfrastructureLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerCommunication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactTarget",
                table: "CommunicationLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "CommunicationLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EvidenceRefs",
                table: "CommunicationLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "CommunicationLogs",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OwnerDecision",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OwnerDecisionAt",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerDecisionChannel",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);

            // Preserve the latest legacy decision for each existing booking.
            migrationBuilder.Sql(@"
                UPDATE b SET b.OwnerDecision = d.Decision, b.OwnerDecisionAt = d.CreatedAt
                FROM Bookings AS b
                CROSS APPLY (
                    SELECT TOP (1) Decision, CreatedAt
                    FROM OwnerDecisions AS od
                    WHERE od.BookingId = b.Id AND od.Decision IN (0, 1)
                    ORDER BY od.CreatedAt DESC, od.Id DESC
                ) AS d;");

            migrationBuilder.CreateIndex(
                name: "IX_CommunicationLogs_BookingId_CreatedAt",
                table: "CommunicationLogs",
                columns: new[] { "BookingId", "CreatedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_CommunicationLogs_Bookings_BookingId",
                table: "CommunicationLogs",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommunicationLogs_Bookings_BookingId",
                table: "CommunicationLogs");

            migrationBuilder.DropIndex(
                name: "IX_CommunicationLogs_BookingId_CreatedAt",
                table: "CommunicationLogs");

            migrationBuilder.DropColumn(
                name: "ContactTarget",
                table: "CommunicationLogs");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "CommunicationLogs");

            migrationBuilder.DropColumn(
                name: "EvidenceRefs",
                table: "CommunicationLogs");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "CommunicationLogs");

            migrationBuilder.DropColumn(
                name: "OwnerDecision",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "OwnerDecisionAt",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "OwnerDecisionChannel",
                table: "Bookings");
        }
    }
}
