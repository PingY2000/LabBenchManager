using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabBenchManager.Migrations
{
    /// <inheritdoc />
    public partial class AddUsersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_Benches_BenchId",
                table: "Assignments");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "Assignments",
                newName: "EstimatedSampleTime");

            migrationBuilder.RenameColumn(
                name: "EndTime",
                table: "Assignments",
                newName: "DesiredCompletionTime");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Assignments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BenchId",
                table: "Assignments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "Assignments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "Assignments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Assignments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsUrgent",
                table: "Assignments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SampleBatchNo",
                table: "Assignments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SampleQuantity",
                table: "Assignments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SampleRequirements",
                table: "Assignments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SampleSpecification",
                table: "Assignments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpecialRequirements",
                table: "Assignments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Stage",
                table: "Assignments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StageDescription",
                table: "Assignments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TestContent",
                table: "Assignments",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TestParameters",
                table: "Assignments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TestStandard",
                table: "Assignments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UrgentReason",
                table: "Assignments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_Benches_BenchId",
                table: "Assignments",
                column: "BenchId",
                principalTable: "Benches",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assignments_Benches_BenchId",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "IsUrgent",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "SampleBatchNo",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "SampleQuantity",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "SampleRequirements",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "SampleSpecification",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "SpecialRequirements",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "Stage",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "StageDescription",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "TestContent",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "TestParameters",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "TestStandard",
                table: "Assignments");

            migrationBuilder.DropColumn(
                name: "UrgentReason",
                table: "Assignments");

            migrationBuilder.RenameColumn(
                name: "EstimatedSampleTime",
                table: "Assignments",
                newName: "StartTime");

            migrationBuilder.RenameColumn(
                name: "DesiredCompletionTime",
                table: "Assignments",
                newName: "EndTime");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Assignments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "BenchId",
                table: "Assignments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Assignments_Benches_BenchId",
                table: "Assignments",
                column: "BenchId",
                principalTable: "Benches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
