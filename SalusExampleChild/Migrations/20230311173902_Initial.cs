using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalusExampleChild.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExampleData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Data1 = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExampleData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalusSaves",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UpdateDateTimeUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedDateTimeUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FailedMessageSendAttempts = table.Column<int>(type: "INTEGER", nullable: false),
                    LastFailedMessageSendAttemptUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NextMessageSendAttemptUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SaveJson = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalusSaves", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalusSaves_CompletedDateTimeUtc_NextMessageSendAttemptUtc",
                table: "SalusSaves",
                columns: new[] { "CompletedDateTimeUtc", "NextMessageSendAttemptUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExampleData");

            migrationBuilder.DropTable(
                name: "SalusSaves");
        }
    }
}
