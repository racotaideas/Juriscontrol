using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JurisControl.Data.Migrations
{
    /// <inheritdoc />
    public partial class CobranzaCreditosPagosGestionesRemates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Creditos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DespachoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AsuntoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeudorClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NombreDeudor = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    NumeroCredito = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Acreedor = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    MontoOriginal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SaldoActual = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TasaInteres = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: true),
                    FechaOrigen = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaUltimoPago = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaVencimiento = table.Column<DateOnly>(type: "date", nullable: true),
                    DiasMora = table.Column<int>(type: "int", nullable: true),
                    Garantia = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Creditos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Creditos_Asuntos_AsuntoId",
                        column: x => x.AsuntoId,
                        principalTable: "Asuntos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Creditos_Clientes_DeudorClienteId",
                        column: x => x.DeudorClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Creditos_Despachos_DespachoId",
                        column: x => x.DespachoId,
                        principalTable: "Despachos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GestionesCobranza",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DespachoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreditoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Canal = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Resultado = table.Column<int>(type: "int", nullable: false),
                    PersonaContactada = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    PromesaFecha = table.Column<DateOnly>(type: "date", nullable: true),
                    PromesaMonto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    GestorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GestionesCobranza", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GestionesCobranza_AspNetUsers_GestorId",
                        column: x => x.GestorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_GestionesCobranza_Creditos_CreditoId",
                        column: x => x.CreditoId,
                        principalTable: "Creditos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GestionesCobranza_Despachos_DespachoId",
                        column: x => x.DespachoId,
                        principalTable: "Despachos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PagosCobranza",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DespachoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreditoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AplicadoCapital = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AplicadoInteres = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AplicadoGastos = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MedioPago = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Referencia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagosCobranza", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PagosCobranza_Creditos_CreditoId",
                        column: x => x.CreditoId,
                        principalTable: "Creditos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PagosCobranza_Despachos_DespachoId",
                        column: x => x.DespachoId,
                        principalTable: "Despachos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Remates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DespachoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreditoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Almoneda = table.Column<int>(type: "int", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Lugar = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ValorAvaluoBase = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PosturaLegal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    MontoFincado = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Postor = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Remates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Remates_Creditos_CreditoId",
                        column: x => x.CreditoId,
                        principalTable: "Creditos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Remates_Despachos_DespachoId",
                        column: x => x.DespachoId,
                        principalTable: "Despachos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Creditos_AsuntoId",
                table: "Creditos",
                column: "AsuntoId");

            migrationBuilder.CreateIndex(
                name: "IX_Creditos_DespachoId_Acreedor",
                table: "Creditos",
                columns: new[] { "DespachoId", "Acreedor" });

            migrationBuilder.CreateIndex(
                name: "IX_Creditos_DespachoId_Estado",
                table: "Creditos",
                columns: new[] { "DespachoId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_Creditos_DespachoId_NumeroCredito",
                table: "Creditos",
                columns: new[] { "DespachoId", "NumeroCredito" });

            migrationBuilder.CreateIndex(
                name: "IX_Creditos_DeudorClienteId",
                table: "Creditos",
                column: "DeudorClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_GestionesCobranza_CreditoId",
                table: "GestionesCobranza",
                column: "CreditoId");

            migrationBuilder.CreateIndex(
                name: "IX_GestionesCobranza_DespachoId_CreditoId_Fecha",
                table: "GestionesCobranza",
                columns: new[] { "DespachoId", "CreditoId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_GestionesCobranza_GestorId",
                table: "GestionesCobranza",
                column: "GestorId");

            migrationBuilder.CreateIndex(
                name: "IX_PagosCobranza_CreditoId",
                table: "PagosCobranza",
                column: "CreditoId");

            migrationBuilder.CreateIndex(
                name: "IX_PagosCobranza_DespachoId_CreditoId_Fecha",
                table: "PagosCobranza",
                columns: new[] { "DespachoId", "CreditoId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_Remates_CreditoId",
                table: "Remates",
                column: "CreditoId");

            migrationBuilder.CreateIndex(
                name: "IX_Remates_DespachoId_CreditoId_Almoneda",
                table: "Remates",
                columns: new[] { "DespachoId", "CreditoId", "Almoneda" });

            migrationBuilder.CreateIndex(
                name: "IX_Remates_DespachoId_FechaHora",
                table: "Remates",
                columns: new[] { "DespachoId", "FechaHora" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GestionesCobranza");

            migrationBuilder.DropTable(
                name: "PagosCobranza");

            migrationBuilder.DropTable(
                name: "Remates");

            migrationBuilder.DropTable(
                name: "Creditos");
        }
    }
}
