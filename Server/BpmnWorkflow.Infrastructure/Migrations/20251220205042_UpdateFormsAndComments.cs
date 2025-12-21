using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BpmnWorkflow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFormsAndComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Workflows_WorkflowId",
                table: "Comments");

            migrationBuilder.AlterColumn<Guid>(
                name: "WorkflowId",
                table: "Comments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "FormId",
                table: "Comments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FormVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FormId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SchemaJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormVersions_FormDefinitions_FormId",
                        column: x => x.FormId,
                        principalTable: "FormDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_FormId",
                table: "Comments",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_FormVersions_FormId",
                table: "FormVersions",
                column: "FormId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_FormDefinitions_FormId",
                table: "Comments",
                column: "FormId",
                principalTable: "FormDefinitions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Workflows_WorkflowId",
                table: "Comments",
                column: "WorkflowId",
                principalTable: "Workflows",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_FormDefinitions_FormId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Workflows_WorkflowId",
                table: "Comments");

            migrationBuilder.DropTable(
                name: "FormVersions");

            migrationBuilder.DropIndex(
                name: "IX_Comments_FormId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "FormId",
                table: "Comments");

            migrationBuilder.AlterColumn<Guid>(
                name: "WorkflowId",
                table: "Comments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Workflows_WorkflowId",
                table: "Comments",
                column: "WorkflowId",
                principalTable: "Workflows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
