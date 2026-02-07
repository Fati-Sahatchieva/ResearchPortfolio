using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResearchPortfolio.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByUserToPublication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Publications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Publications");
        }
    }
}
