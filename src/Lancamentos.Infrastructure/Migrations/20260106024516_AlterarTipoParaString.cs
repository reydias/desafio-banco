using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lancamentos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlterarTipoParaString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Se a tabela já existir com dados, converter valores: 1 -> "C", 2 -> "D"
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Lancamentos')
                BEGIN
                    UPDATE [Lancamentos] SET [Tipo] = 'C' WHERE [Tipo] = 1;
                    UPDATE [Lancamentos] SET [Tipo] = 'D' WHERE [Tipo] = 2;
                END
            ");

            migrationBuilder.AlterColumn<string>(
                name: "Tipo",
                table: "Lancamentos",
                type: "nvarchar(1)",
                maxLength: 1,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Converter valores de volta: "C" -> 1, "D" -> 2
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Lancamentos')
                BEGIN
                    UPDATE [Lancamentos] SET [Tipo] = 1 WHERE [Tipo] = 'C';
                    UPDATE [Lancamentos] SET [Tipo] = 2 WHERE [Tipo] = 'D';
                END
            ");

            migrationBuilder.AlterColumn<int>(
                name: "Tipo",
                table: "Lancamentos",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1)");
        }
    }
}
