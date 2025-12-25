using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BpmnWorkflow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CamundaMIG : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProcessInstances_Workflows_WorkflowId",
                table: "ProcessInstances");

            migrationBuilder.AlterColumn<Guid>(
                name: "WorkflowId",
                table: "ProcessInstances",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

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

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "CamundaEnvironments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "CamundaEnvironments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_ProcessInstances_Workflows_WorkflowId",
                table: "ProcessInstances",
                column: "WorkflowId",
                principalTable: "Workflows",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProcessInstances_Workflows_WorkflowId",
                table: "ProcessInstances");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "CamundaEnvironments");

            migrationBuilder.DropColumn(
                name: "ClientSecret",
                table: "CamundaEnvironments");

            migrationBuilder.DropColumn(
                name: "ClusterId",
                table: "CamundaEnvironments");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "CamundaEnvironments");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "CamundaEnvironments");

            migrationBuilder.AlterColumn<Guid>(
                name: "WorkflowId",
                table: "ProcessInstances",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProcessInstances_Workflows_WorkflowId",
                table: "ProcessInstances",
                column: "WorkflowId",
                principalTable: "Workflows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
