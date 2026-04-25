using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Drippin.Migrations
{
    /// <inheritdoc />
    public partial class DrippinTandas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UsNombre",
                table: "Usuario",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "UsCorreo",
                table: "Usuario",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "UsApellido",
                table: "Usuario",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Usuario",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "proTandaId",
                table: "Producto",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Tanda",
                columns: table => new
                {
                    TandaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TanNombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TanVisible = table.Column<bool>(type: "bit", nullable: false),
                    TanFechaInicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TanFechaFin = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tanda", x => x.TandaId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Producto_proTandaId",
                table: "Producto",
                column: "proTandaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Producto_Tanda_proTandaId",
                table: "Producto",
                column: "proTandaId",
                principalTable: "Tanda",
                principalColumn: "TandaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Producto_Tanda_proTandaId",
                table: "Producto");

            migrationBuilder.DropTable(
                name: "Tanda");

            migrationBuilder.DropIndex(
                name: "IX_Producto_proTandaId",
                table: "Producto");

            migrationBuilder.DropColumn(
                name: "proTandaId",
                table: "Producto");

            migrationBuilder.AlterColumn<string>(
                name: "UsNombre",
                table: "Usuario",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "UsCorreo",
                table: "Usuario",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "UsApellido",
                table: "Usuario",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "PasswordHash",
                table: "Usuario",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);
        }
    }
}
