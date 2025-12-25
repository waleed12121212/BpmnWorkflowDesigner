using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BpmnWorkflow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addnewzeebetocamundaMIG : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "CamundaEnvironments");

            migrationBuilder.RenameColumn(
                name: "ZeebeUrl",
                table: "CamundaEnvironments",
                newName: "ZeebeGatewayUrl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ZeebeGatewayUrl",
                table: "CamundaEnvironments",
                newName: "ZeebeUrl");

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "CamundaEnvironments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
