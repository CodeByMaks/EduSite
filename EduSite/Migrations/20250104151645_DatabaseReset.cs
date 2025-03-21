using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduSite.Migrations
{
    /// <inheritdoc />
    public partial class DatabaseReset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFree",
                table: "Courses",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Courses",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFree",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Courses");
        }
    }
}
