using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJointAssessment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "JointAssessments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    AppointmentId = table.Column<int>(type: "int", nullable: true),
                    AssessmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    JointsDataJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalTender = table.Column<int>(type: "int", nullable: false),
                    TotalSwollen = table.Column<int>(type: "int", nullable: false),
                    TotalBoth = table.Column<int>(type: "int", nullable: false),
                    TotalLimited = table.Column<int>(type: "int", nullable: false),
                    TotalNormal = table.Column<int>(type: "int", nullable: false),
                    TotalJointsAssessed = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JointAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JointAssessments_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JointAssessments_PatientId",
                table: "JointAssessments",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JointAssessments");
        }
    }
}
