using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JurisControl.Data.Migrations
{
    /// <inheritdoc />
    public partial class PlantillasGastos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Gastos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DespachoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JuicioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AsuntoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Concepto = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Reembolsable = table.Column<bool>(type: "bit", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Comprobante = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Gastos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Gastos_Asuntos_AsuntoId",
                        column: x => x.AsuntoId,
                        principalTable: "Asuntos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Gastos_Despachos_DespachoId",
                        column: x => x.DespachoId,
                        principalTable: "Despachos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Gastos_Juicios_JuicioId",
                        column: x => x.JuicioId,
                        principalTable: "Juicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Plantillas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DespachoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Clave = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Cuerpo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plantillas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Plantillas_Despachos_DespachoId",
                        column: x => x.DespachoId,
                        principalTable: "Despachos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Gastos_AsuntoId",
                table: "Gastos",
                column: "AsuntoId");

            migrationBuilder.CreateIndex(
                name: "IX_Gastos_DespachoId_AsuntoId",
                table: "Gastos",
                columns: new[] { "DespachoId", "AsuntoId" });

            migrationBuilder.CreateIndex(
                name: "IX_Gastos_DespachoId_JuicioId_Fecha",
                table: "Gastos",
                columns: new[] { "DespachoId", "JuicioId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_Gastos_JuicioId",
                table: "Gastos",
                column: "JuicioId");

            migrationBuilder.CreateIndex(
                name: "IX_Plantillas_DespachoId_Activa",
                table: "Plantillas",
                columns: new[] { "DespachoId", "Activa" });

            migrationBuilder.CreateIndex(
                name: "IX_Plantillas_DespachoId_Categoria",
                table: "Plantillas",
                columns: new[] { "DespachoId", "Categoria" });

            migrationBuilder.CreateIndex(
                name: "IX_Plantillas_DespachoId_Clave",
                table: "Plantillas",
                columns: new[] { "DespachoId", "Clave" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Gastos");

            migrationBuilder.DropTable(
                name: "Plantillas");
        }
    }
}
