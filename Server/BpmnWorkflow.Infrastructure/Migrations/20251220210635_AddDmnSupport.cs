using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BpmnWorkflow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDmnSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DmnDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DmnXml = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DmnDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DmnDefinitions_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DmnVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DmnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DmnXml = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DmnVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DmnVersions_DmnDefinitions_DmnId",
                        column: x => x.DmnId,
                        principalTable: "DmnDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DmnDefinitions_OwnerId",
                table: "DmnDefinitions",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_DmnVersions_DmnId",
                table: "DmnVersions",
                column: "DmnId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DmnVersions");

            migrationBuilder.DropTable(
                name: "DmnDefinitions");
        }
    }
}
