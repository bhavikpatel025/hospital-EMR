using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAiExplanations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiExplanationEnglish",
                table: "PatientDocuments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AiExplanationGujarati",
                table: "PatientDocuments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AiExplanationHindi",
                table: "PatientDocuments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiExplanationEnglish",
                table: "PatientDocuments");

            migrationBuilder.DropColumn(
                name: "AiExplanationGujarati",
                table: "PatientDocuments");

            migrationBuilder.DropColumn(
                name: "AiExplanationHindi",
                table: "PatientDocuments");
        }
    }
}
