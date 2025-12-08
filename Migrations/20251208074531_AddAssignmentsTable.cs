using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabBenchManager.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignmentsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Benches");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Benches",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Benches",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Benches",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "CurrentUser",
                table: "Benches",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextAvailableTime",
                table: "Benches",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Project",
                table: "Benches",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Benches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Assignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicantName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProjectName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BenchId = table.Column<int>(type: "int", nullable: false),
                    RequestTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assignments_Benches_BenchId",
                        column: x => x.BenchId,
                        principalTable: "Benches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_BenchId",
                table: "Assignments",
                column: "BenchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assignments");

            migrationBuilder.DropColumn(
                name: "CurrentUser",
                table: "Benches");

            migrationBuilder.DropColumn(
                name: "NextAvailableTime",
                table: "Benches");

            migrationBuilder.DropColumn(
                name: "Project",
                table: "Benches");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Benches");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Benches",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Location",
                table: "Benches",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Benches",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Benches",
                type: "TEXT",
                nullable: true);
        }
    }
}
