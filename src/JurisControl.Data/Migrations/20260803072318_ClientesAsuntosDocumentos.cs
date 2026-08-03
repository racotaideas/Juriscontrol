using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JurisControl.Data.Migrations
{
    /// <inheritdoc />
    public partial class ClientesAsuntosDocumentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Asuntos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DespachoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Folio = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    MateriaKey = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ResponsableId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaRecepcion = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    FechaCierre = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    NotasPrivadas = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Cuantia = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Prioridad = table.Column<int>(type: "int", nullable: false),
                    Etiquetas = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EsCobranza = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asuntos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Asuntos_AspNetUsers_ResponsableId",
                        column: x => x.ResponsableId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Asuntos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Asuntos_Despachos_DespachoId",
                        column: x => x.DespachoId,
                        principalTable: "Despachos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContadoresFolio",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DespachoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Anio = table.Column<int>(type: "int", nullable: false),
                    UltimoNumero = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContadoresFolio", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Documentos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DespachoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AsuntoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Nombre = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StorageRef = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    TamanoBytes = table.Column<long>(type: "bigint", nullable: false),
                    Notas = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documentos_Asuntos_AsuntoId",
                        column: x => x.AsuntoId,
                        principalTable: "Asuntos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Documentos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Documentos_Despachos_DespachoId",
                        column: x => x.DespachoId,
                        principalTable: "Despachos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Asuntos_ClienteId",
                table: "Asuntos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Asuntos_DespachoId_ClienteId",
                table: "Asuntos",
                columns: new[] { "DespachoId", "ClienteId" });

            migrationBuilder.CreateIndex(
                name: "IX_Asuntos_DespachoId_Estado",
                table: "Asuntos",
                columns: new[] { "DespachoId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_Asuntos_DespachoId_Folio",
                table: "Asuntos",
                columns: new[] { "DespachoId", "Folio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Asuntos_DespachoId_MateriaKey",
                table: "Asuntos",
                columns: new[] { "DespachoId", "MateriaKey" });

            migrationBuilder.CreateIndex(
                name: "IX_Asuntos_DespachoId_ResponsableId",
                table: "Asuntos",
                columns: new[] { "DespachoId", "ResponsableId" });

            migrationBuilder.CreateIndex(
                name: "IX_Asuntos_ResponsableId",
                table: "Asuntos",
                column: "ResponsableId");

            migrationBuilder.CreateIndex(
                name: "IX_ContadoresFolio_DespachoId_Anio",
                table: "ContadoresFolio",
                columns: new[] { "DespachoId", "Anio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_AsuntoId",
                table: "Documentos",
                column: "AsuntoId");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_ClienteId",
                table: "Documentos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_DespachoId_AsuntoId",
                table: "Documentos",
                columns: new[] { "DespachoId", "AsuntoId" });

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_DespachoId_ClienteId",
                table: "Documentos",
                columns: new[] { "DespachoId", "ClienteId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContadoresFolio");

            migrationBuilder.DropTable(
                name: "Documentos");

            migrationBuilder.DropTable(
                name: "Asuntos");
        }
    }
}
