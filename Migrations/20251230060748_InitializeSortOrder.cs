using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabBenchManager.Migrations
{
    /// <inheritdoc />
    public partial class InitializeSortOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                WITH Numbered AS (
                    SELECT Id, ROW_NUMBER() OVER (ORDER BY Id) - 1 AS RowNum
                    FROM Benches
                )
                UPDATE Benches 
                SET SortOrder = Numbered.RowNum
                FROM Benches
                INNER JOIN Numbered ON Benches.Id = Numbered.Id
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE Benches SET SortOrder = 0");
        }
    }
}
