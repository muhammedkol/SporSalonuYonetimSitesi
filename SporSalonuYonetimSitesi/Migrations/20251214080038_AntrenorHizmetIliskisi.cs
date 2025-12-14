using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SporSalonuYonetimSitesi.Migrations
{
    /// <inheritdoc />
    public partial class AntrenorHizmetIliskisi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AntrenorHizmet",
                columns: table => new
                {
                    AntrenorlerAntrenorId = table.Column<int>(type: "int", nullable: false),
                    HizmetlerHizmetId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AntrenorHizmet", x => new { x.AntrenorlerAntrenorId, x.HizmetlerHizmetId });
                    table.ForeignKey(
                        name: "FK_AntrenorHizmet_Antrenorler_AntrenorlerAntrenorId",
                        column: x => x.AntrenorlerAntrenorId,
                        principalTable: "Antrenorler",
                        principalColumn: "AntrenorId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AntrenorHizmet_Hizmetler_HizmetlerHizmetId",
                        column: x => x.HizmetlerHizmetId,
                        principalTable: "Hizmetler",
                        principalColumn: "HizmetId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AntrenorHizmet_HizmetlerHizmetId",
                table: "AntrenorHizmet",
                column: "HizmetlerHizmetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AntrenorHizmet");
        }
    }
}
