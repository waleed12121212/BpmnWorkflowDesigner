using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BpmnWorkflow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addzeebetocamundaMIG : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "CamundaEnvironments");

            migrationBuilder.DropColumn(
                name: "ClientSecret",
                table: "CamundaEnvironments");

            migrationBuilder.DropColumn(
                name: "ClusterId",
                table: "CamundaEnvironments");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "CamundaEnvironments",
                newName: "Version");

            migrationBuilder.RenameColumn(
                name: "Region",
                table: "CamundaEnvironments",
                newName: "ZeebeUrl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ZeebeUrl",
                table: "CamundaEnvironments",
                newName: "Region");

            migrationBuilder.RenameColumn(
                name: "Version",
                table: "CamundaEnvironments",
                newName: "Type");

            migrationBuilder.AddColumn<string>(
                name: "ClientId",
                table: "CamundaEnvironments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientSecret",
                table: "CamundaEnvironments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClusterId",
                table: "CamundaEnvironments",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
