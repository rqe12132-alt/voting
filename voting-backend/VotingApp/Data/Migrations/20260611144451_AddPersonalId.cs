using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VotingApp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonalId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PersonalIds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalIds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonalIds_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonalIds_Number",
                table: "PersonalIds",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonalIds_UserId",
                table: "PersonalIds",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonalIds");
        }
    }
}
