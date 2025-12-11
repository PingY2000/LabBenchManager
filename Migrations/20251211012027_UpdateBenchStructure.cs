using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabBenchManager.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBenchStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BasicPerformance",
                table: "Benches");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Benches");

            migrationBuilder.RenameColumn(
                name: "EquipmentAssetNo",
                table: "Benches",
                newName: "EquipmentNo");

            migrationBuilder.AddColumn<string>(
                name: "AssetNo",
                table: "Benches",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BasicPerformanceAndConfiguration",
                table: "Benches",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssetNo",
                table: "Benches");

            migrationBuilder.DropColumn(
                name: "BasicPerformanceAndConfiguration",
                table: "Benches");

            migrationBuilder.RenameColumn(
                name: "EquipmentNo",
                table: "Benches",
                newName: "EquipmentAssetNo");

            migrationBuilder.AddColumn<string>(
                name: "BasicPerformance",
                table: "Benches",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Benches",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
