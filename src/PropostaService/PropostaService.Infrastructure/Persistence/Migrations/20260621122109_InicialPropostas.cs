using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropostaService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InicialPropostas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "proposta_numero_seq");

            migrationBuilder.CreateTable(
                name: "Propostas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Numero = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ClienteNome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ClienteDocumento = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    ClienteEmail = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    ValorCobertura = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ValorPremio = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DataCriacao = table.Column<DateTimeOffset>(type: "timestamptz", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Propostas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Propostas_ClienteDocumento",
                table: "Propostas",
                column: "ClienteDocumento");

            migrationBuilder.CreateIndex(
                name: "IX_Propostas_Numero",
                table: "Propostas",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Propostas_Status",
                table: "Propostas",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Propostas");

            migrationBuilder.DropSequence(
                name: "proposta_numero_seq");
        }
    }
}
