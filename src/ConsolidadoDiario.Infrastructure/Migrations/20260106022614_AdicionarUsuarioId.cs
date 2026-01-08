using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsolidadoDiario.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarUsuarioId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Consolidados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Data = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalCreditos = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDebitos = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SaldoDiario = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    QuantidadeLancamentos = table.Column<int>(type: "int", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consolidados", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Consolidados_UsuarioId",
                table: "Consolidados",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Consolidados_UsuarioId_Data",
                table: "Consolidados",
                columns: new[] { "UsuarioId", "Data" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Consolidados");
        }
    }
}
