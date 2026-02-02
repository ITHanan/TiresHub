using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfrastructureLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop any self-FK on Branches (doesn't assume FK name)
            migrationBuilder.Sql(@"
DECLARE @fkName nvarchar(200);

SELECT @fkName = fk.name
FROM sys.foreign_keys fk
WHERE fk.parent_object_id = OBJECT_ID(N'dbo.Branches')
  AND fk.referenced_object_id = OBJECT_ID(N'dbo.Branches');

IF @fkName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE [dbo].[Branches] DROP CONSTRAINT [' + @fkName + ']');
END
");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Branches_BranchId",
                table: "Users");

            migrationBuilder.Sql(@"
IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Branches_BranchId'
      AND object_id = OBJECT_ID('dbo.Branches')
)
BEGIN
    DROP INDEX [IX_Branches_BranchId] ON [dbo].[Branches];
END
");

            migrationBuilder.Sql(@"
IF COL_LENGTH('dbo.Branches', 'BranchId') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[Branches] DROP COLUMN [BranchId];
END
");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TireType",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Branches_BranchId",
                table: "Users",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Branches_BranchId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "TireType",
                table: "Bookings");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "Branches",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Branches_BranchId",
                table: "Branches",
                column: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Branches_Branches_BranchId",
                table: "Branches",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Branches_BranchId",
                table: "Users",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");
        }
    }
}
