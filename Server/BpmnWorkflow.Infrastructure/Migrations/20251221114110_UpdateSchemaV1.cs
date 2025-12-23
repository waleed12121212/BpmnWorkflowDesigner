using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BpmnWorkflow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchemaV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CamundaDeploymentId",
                table: "Workflows",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CamundaProcessDefinitionId",
                table: "Workflows",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeployed",
                table: "Workflows",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastDeployedAt",
                table: "Workflows",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProcessInstances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WorkflowId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CamundaProcessInstanceId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CamundaProcessDefinitionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BusinessKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    State = table.Column<int>(type: "int", nullable: false),
                    IsSuspended = table.Column<bool>(type: "bit", nullable: false),
                    Variables = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EndReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessInstances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessInstances_Users_StartedByUserId",
                        column: x => x.StartedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProcessInstances_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessInstances_StartedByUserId",
                table: "ProcessInstances",
                column: "StartedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessInstances_WorkflowId",
                table: "ProcessInstances",
                column: "WorkflowId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessInstances");

            migrationBuilder.DropColumn(
                name: "CamundaDeploymentId",
                table: "Workflows");

            migrationBuilder.DropColumn(
                name: "CamundaProcessDefinitionId",
                table: "Workflows");

            migrationBuilder.DropColumn(
                name: "IsDeployed",
                table: "Workflows");

            migrationBuilder.DropColumn(
                name: "LastDeployedAt",
                table: "Workflows");
        }
    }
}
