using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLabFindingStatusAndCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "PatientLabFindings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Normal");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "PatientLabFindings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "General");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "PatientLabFindings");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "PatientLabFindings");
        }
    }
}
