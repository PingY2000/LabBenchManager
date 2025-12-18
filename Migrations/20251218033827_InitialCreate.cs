using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabBenchManager.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NtAccount = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Benches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EquipmentNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AssetNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TestType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TestObject = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    WorkingHoursNorm = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BasicPerformanceAndConfiguration = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PictureUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CurrentUser = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Project = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NextAvailableTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Benches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Assignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicantNTAccount = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ApplicantName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProjectName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BenchId = table.Column<int>(type: "int", nullable: true),
                    TestPlanId = table.Column<int>(type: "int", nullable: true),
                    RequestTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstimatedSampleTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DesiredCompletionTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SampleQuantity = table.Column<int>(type: "int", nullable: false),
                    SampleSpecification = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SampleBatchNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SampleRequirements = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TestContent = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    TestStandard = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TestParameters = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    StageDescription = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SpecialRequirements = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsUrgent = table.Column<bool>(type: "bit", nullable: false),
                    UrgentReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assignments_Benches_BenchId",
                        column: x => x.BenchId,
                        principalTable: "Benches",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TestPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BenchId = table.Column<int>(type: "int", nullable: false),
                    AssignmentId = table.Column<int>(type: "int", nullable: true),
                    ProjectName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ScheduledDates = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AssignedTo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RequestedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SampleNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SampleQuantity = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestPlans_Assignments_AssignmentId",
                        column: x => x.AssignmentId,
                        principalTable: "Assignments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TestPlans_Benches_BenchId",
                        column: x => x.BenchId,
                        principalTable: "Benches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_BenchId",
                table: "Assignments",
                column: "BenchId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_TestPlanId",
                table: "Assignments",
                column: "TestPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_TestPlans_AssignmentId",
                table: "TestPlans",
                column: "AssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TestPlans_BenchId",
                table: "TestPlans",
                column: "BenchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_TestPlans_TestPlanId",
                table: "Assignments",
                column: "TestPlanId",
                principalTable: "TestPlans",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_Benches_BenchId",
                table: "Assignments");

            migrationBuilder.DropForeignKey(
                name: "FK_TestPlans_Benches_BenchId",
                table: "TestPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_TestPlans_TestPlanId",
                table: "Assignments");

            migrationBuilder.DropTable(
                name: "ApplicationUsers");

            migrationBuilder.DropTable(
                name: "Benches");

            migrationBuilder.DropTable(
                name: "TestPlans");

            migrationBuilder.DropTable(
                name: "Assignments");
        }
    }
}
