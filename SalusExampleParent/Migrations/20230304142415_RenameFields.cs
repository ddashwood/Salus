using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalusExampleParent.Migrations
{
    /// <inheritdoc />
    public partial class RenameFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalusDataChanges");

            migrationBuilder.CreateTable(
                name: "SalusSaves",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
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
                name: "SalusSaves");

            migrationBuilder.CreateTable(
                name: "SalusDataChanges",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    CompletedDateTimeUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FailedMessageSendAttempts = table.Column<int>(type: "INTEGER", nullable: false),
                    LastFailedMessageSendAttemptUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NextMessageSendAttemptUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateDateTimeUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdateJson = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalusDataChanges", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalusDataChanges_CompletedDateTimeUtc_NextMessageSendAttemptUtc",
                table: "SalusDataChanges",
                columns: new[] { "CompletedDateTimeUtc", "NextMessageSendAttemptUtc" });
        }
    }
}
