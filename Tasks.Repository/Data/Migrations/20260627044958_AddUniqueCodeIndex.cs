using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tasks.Repository.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueCodeIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Corporations_Code",
                table: "Corporations",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Corporations_Code",
                table: "Corporations");
        }
    }
}
