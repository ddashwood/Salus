using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalusExampleParent.Migrations
{
    /// <inheritdoc />
    public partial class AddExampleData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SalusDataChanges_CompletedDateTimeUtc_FailedMessageSendAttempts",
                table: "SalusDataChanges");

            migrationBuilder.CreateTable(
                name: "ExampleData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Data = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExampleData", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalusDataChanges_CompletedDateTimeUtc_NextMessageSendAttemptUtc",
                table: "SalusDataChanges",
                columns: new[] { "CompletedDateTimeUtc", "NextMessageSendAttemptUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExampleData");

            migrationBuilder.DropIndex(
                name: "IX_SalusDataChanges_CompletedDateTimeUtc_NextMessageSendAttemptUtc",
                table: "SalusDataChanges");

            migrationBuilder.CreateIndex(
                name: "IX_SalusDataChanges_CompletedDateTimeUtc_FailedMessageSendAttempts",
                table: "SalusDataChanges",
                columns: new[] { "CompletedDateTimeUtc", "FailedMessageSendAttempts" });
        }
    }
}
