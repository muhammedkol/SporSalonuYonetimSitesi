using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SporSalonuYonetimSitesi.Migrations
{
    /// <inheritdoc />
    public partial class NewUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Randevular_Antrenorler_AntrenorId",
                table: "Randevular");

            migrationBuilder.DropColumn(
                name: "AntrenörId",
                table: "Randevular");

            migrationBuilder.AlterColumn<int>(
                name: "AntrenorId",
                table: "Randevular",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Randevular_Antrenorler_AntrenorId",
                table: "Randevular",
                column: "AntrenorId",
                principalTable: "Antrenorler",
                principalColumn: "AntrenorId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Randevular_Antrenorler_AntrenorId",
                table: "Randevular");

            migrationBuilder.AlterColumn<int>(
                name: "AntrenorId",
                table: "Randevular",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "AntrenörId",
                table: "Randevular",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Randevular_Antrenorler_AntrenorId",
                table: "Randevular",
                column: "AntrenorId",
                principalTable: "Antrenorler",
                principalColumn: "AntrenorId");
        }
    }
}
