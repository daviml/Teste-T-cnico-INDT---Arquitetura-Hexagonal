using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContratacaoService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InicialContratacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contratacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PropostaId = table.Column<Guid>(type: "uuid", nullable: false),
                    NumeroApolice = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ValorPremioPago = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DataContratacao = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contratacoes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contratacoes_NumeroApolice",
                table: "Contratacoes",
                column: "NumeroApolice",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contratacoes_PropostaId",
                table: "Contratacoes",
                column: "PropostaId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contratacoes");
        }
    }
}
