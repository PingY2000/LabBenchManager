using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabBenchManager.Migrations
{
    /// <inheritdoc />
    public partial class AddExtendedBenchProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BasicPerformance",
                table: "Benches",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EquipmentAssetNo",
                table: "Benches",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PictureUrl",
                table: "Benches",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Benches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "TestObject",
                table: "Benches",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TestType",
                table: "Benches",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkingHoursNorm",
                table: "Benches",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BasicPerformance",
                table: "Benches");

            migrationBuilder.DropColumn(
                name: "EquipmentAssetNo",
                table: "Benches");

            migrationBuilder.DropColumn(
                name: "PictureUrl",
                table: "Benches");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Benches");

            migrationBuilder.DropColumn(
                name: "TestObject",
                table: "Benches");

            migrationBuilder.DropColumn(
                name: "TestType",
                table: "Benches");

            migrationBuilder.DropColumn(
                name: "WorkingHoursNorm",
                table: "Benches");
        }
    }
}
