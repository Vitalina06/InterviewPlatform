using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InterviewPlatform.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddArchive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Interviews",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Candidates",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Candidates");
        }
    }
}
