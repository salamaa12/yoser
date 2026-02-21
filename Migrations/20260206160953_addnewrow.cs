using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yoser_API.Migrations
{
    /// <inheritdoc />
    public partial class addnewrow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MedicalCondition",
                table: "PatientProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MedicalCondition",
                table: "PatientProfiles");
        }
    }
}
