using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfrastructureLayer.Migrations
{
    /// <inheritdoc />
    public partial class US7_BranchManagers_M2M : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Branches_BranchId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_BranchId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Users");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "Branches",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BranchManagers",
                columns: table => new
                {
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShopManagerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchManagers", x => new { x.BranchId, x.ShopManagerId });
                    table.ForeignKey(
                        name: "FK_BranchManagers_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BranchManagers_Users_ShopManagerId",
                        column: x => x.ShopManagerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Branches_BranchId",
                table: "Branches",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchManagers_ShopManagerId",
                table: "BranchManagers",
                column: "ShopManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Branches_Branches_BranchId",
                table: "Branches",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Branches_Branches_BranchId",
                table: "Branches");

            migrationBuilder.DropTable(
                name: "BranchManagers");

            migrationBuilder.DropIndex(
                name: "IX_Branches_BranchId",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Branches");

            migrationBuilder.AddColumn<Guid>(
                name: "BranchId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_BranchId",
                table: "Users",
                column: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Branches_BranchId",
                table: "Users",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id");
        }
    }
}
