using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabBenchManager.Migrations
{
    /// <inheritdoc />
    public partial class Correlate1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssignmentId",
                table: "TestPlans",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TestPlanId",
                table: "Assignments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestPlans_AssignmentId",
                table: "TestPlans",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_TestPlanId",
                table: "Assignments",
                column: "TestPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_TestPlans_TestPlanId",
                table: "Assignments",
                column: "TestPlanId",
                principalTable: "TestPlans",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TestPlans_Assignments_AssignmentId",
                table: "TestPlans",
                column: "AssignmentId",
                principalTable: "Assignments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_TestPlans_TestPlanId",
                table: "Assignments");

            migrationBuilder.DropForeignKey(
                name: "FK_TestPlans_Assignments_AssignmentId",
                table: "TestPlans");

            migrationBuilder.DropIndex(
                name: "IX_TestPlans_AssignmentId",
                table: "TestPlans");

            migrationBuilder.DropIndex(
                name: "IX_Assignments_TestPlanId",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "AssignmentId",
                table: "TestPlans");

            migrationBuilder.DropColumn(
                name: "TestPlanId",
                table: "Assignments");
        }
    }
}
