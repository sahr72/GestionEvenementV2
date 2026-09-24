using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "evenements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Titre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    PersonneImpliquee = table.Column<string>(type: "text", nullable: false),
                    Statut = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DateCreationUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DateMiseAjourUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evenements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "commentaires",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    DateCreationUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DateMiseAJourUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_commentaires", x => x.Id);
                    table.ForeignKey(
                        name: "FK_commentaires_evenements_EventId",
                        column: x => x.EventId,
                        principalTable: "evenements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_commentaires_EventId",
                table: "commentaires",
                column: "EventId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "commentaires");

            migrationBuilder.DropTable(
                name: "evenements");
        }
    }
}
