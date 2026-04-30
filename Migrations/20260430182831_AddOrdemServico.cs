using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OficinaMecanica.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddOrdemServico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrdensServico",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ClienteId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    VeiculoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StatusAprovacaoOrcamento = table.Column<int>(type: "int", nullable: false),
                    ValorTotalServicos = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    ValorTotalPecasInsumos = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    ValorTotalOrcamento = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    MockEnvioOrcamento = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MotivoRecusaOrcamento = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CriadaEm = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DiagnosticoEm = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    OrcamentoEnviadoEm = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ExecucaoIniciadaEm = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    FinalizadaEm = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EntregueEm = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdensServico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdensServico_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrdensServico_Veiculos_VeiculoId",
                        column: x => x.VeiculoId,
                        principalTable: "Veiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrdemServicoItensPecaInsumo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OrdemServicoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PecaInsumoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    NomePecaInsumo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PrecoUnitario = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Quantidade = table.Column<int>(type: "int", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(65,30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdemServicoItensPecaInsumo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdemServicoItensPecaInsumo_OrdensServico_OrdemServicoId",
                        column: x => x.OrdemServicoId,
                        principalTable: "OrdensServico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrdemServicoItensServico",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OrdemServicoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ServicoId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    NomeServico = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PrecoServico = table.Column<decimal>(type: "decimal(65,30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdemServicoItensServico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdemServicoItensServico_OrdensServico_OrdemServicoId",
                        column: x => x.OrdemServicoId,
                        principalTable: "OrdensServico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_OrdemServicoItensPecaInsumo_OrdemServicoId",
                table: "OrdemServicoItensPecaInsumo",
                column: "OrdemServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdemServicoItensServico_OrdemServicoId",
                table: "OrdemServicoItensServico",
                column: "OrdemServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdensServico_ClienteId",
                table: "OrdensServico",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdensServico_VeiculoId",
                table: "OrdensServico",
                column: "VeiculoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrdemServicoItensPecaInsumo");

            migrationBuilder.DropTable(
                name: "OrdemServicoItensServico");

            migrationBuilder.DropTable(
                name: "OrdensServico");
        }
    }
}
