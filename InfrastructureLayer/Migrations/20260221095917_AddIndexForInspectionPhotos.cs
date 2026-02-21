using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfrastructureLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexForInspectionPhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "InspectionId",
                table: "InspectionReports",
                newName: "BookingId");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByEmployeeId",
                table: "InspectionReports",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InspectionReports_BookingId",
                table: "InspectionReports",
                column: "BookingId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InspectionReports_BookingId",
                table: "InspectionReports");

            migrationBuilder.DropColumn(
                name: "CreatedByEmployeeId",
                table: "InspectionReports");

            migrationBuilder.RenameColumn(
                name: "BookingId",
                table: "InspectionReports",
                newName: "InspectionId");
        }
    }
}
