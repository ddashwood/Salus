using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalusExampleParent.Migrations
{
    /// <inheritdoc />
    public partial class AddExtraDataColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Data",
                table: "ExampleData",
                newName: "Data2");

            migrationBuilder.AddColumn<string>(
                name: "Data1",
                table: "ExampleData",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Data1",
                table: "ExampleData");

            migrationBuilder.RenameColumn(
                name: "Data2",
                table: "ExampleData",
                newName: "Data");
        }
    }
}
