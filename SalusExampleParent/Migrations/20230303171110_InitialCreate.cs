using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalusExampleParent.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalusDataChanges",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    UpdateDateTimeUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedDateTimeUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FailedMessageSendAttempts = table.Column<int>(type: "INTEGER", nullable: false),
                    LastFailedMessageSendAttemptUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NextMessageSendAttemptUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdateJson = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalusDataChanges", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalusDataChanges_CompletedDateTimeUtc_FailedMessageSendAttempts",
                table: "SalusDataChanges",
                columns: new[] { "CompletedDateTimeUtc", "FailedMessageSendAttempts" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalusDataChanges");
        }
    }
}
