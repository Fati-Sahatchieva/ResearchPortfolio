using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResearchPortfolio.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameDescriptionToAbstract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Publications",
                newName: "Abstract");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Abstract",
                table: "Publications",
                newName: "Description");
        }
    }
}
