using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRxAiExplanations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiExplanationEnglish",
                table: "Prescriptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AiExplanationGujarati",
                table: "Prescriptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AiExplanationHindi",
                table: "Prescriptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiExplanationEnglish",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "AiExplanationGujarati",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "AiExplanationHindi",
                table: "Prescriptions");
        }
    }
}
