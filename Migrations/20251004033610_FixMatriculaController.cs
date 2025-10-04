using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PARCIAL.Migrations
{
    /// <inheritdoc />
    public partial class FixMatriculaController : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matriculas_AspNetUsers_UsuarioId",
                table: "Matriculas");

            migrationBuilder.DropForeignKey(
                name: "FK_Matriculas_AspNetUsers_UsuarioId1",
                table: "Matriculas");

            migrationBuilder.DropIndex(
                name: "IX_Matriculas_UsuarioId",
                table: "Matriculas");

            migrationBuilder.DropIndex(
                name: "IX_Matriculas_UsuarioId1",
                table: "Matriculas");

            migrationBuilder.DropColumn(
                name: "UsuarioId1",
                table: "Matriculas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UsuarioId1",
                table: "Matriculas",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Matriculas_UsuarioId",
                table: "Matriculas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Matriculas_UsuarioId1",
                table: "Matriculas",
                column: "UsuarioId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Matriculas_AspNetUsers_UsuarioId",
                table: "Matriculas",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Matriculas_AspNetUsers_UsuarioId1",
                table: "Matriculas",
                column: "UsuarioId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
