using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JurisControl.Data.Migrations
{
    /// <inheritdoc />
    public partial class JuiciosActuacionesAudienciasPlazos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Juicios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DespachoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AsuntoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroExpediente = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Juzgado = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TipoJuicio = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MateriaKey = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaConclusion = table.Column<DateOnly>(type: "date", nullable: true),
                    Cuantia = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Juicios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Juicios_Asuntos_AsuntoId",
                        column: x => x.AsuntoId,
                        principalTable: "Asuntos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Juicios_Despachos_DespachoId",
                        column: x => x.DespachoId,
                        principalTable: "Despachos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Actuaciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DespachoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JuicioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaNotificacion = table.Column<DateOnly>(type: "date", nullable: true),
                    Resumen = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Detalle = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    PlazoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actuaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Actuaciones_Despachos_DespachoId",
                        column: x => x.DespachoId,
                        principalTable: "Despachos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Actuaciones_Juicios_JuicioId",
                        column: x => x.JuicioId,
                        principalTable: "Juicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Audiencias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DespachoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JuicioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Duracion = table.Column<TimeSpan>(type: "time", nullable: true),
                    Tipo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Lugar = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    AsignadoAId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Resultado = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FechaDiferida = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audiencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Audiencias_AspNetUsers_AsignadoAId",
                        column: x => x.AsignadoAId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Audiencias_Despachos_DespachoId",
                        column: x => x.DespachoId,
                        principalTable: "Despachos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Audiencias_Juicios_JuicioId",
                        column: x => x.JuicioId,
                        principalTable: "Juicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartesJuicio",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DespachoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JuicioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rol = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NombreLibre = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Representante = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notas = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartesJuicio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartesJuicio_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PartesJuicio_Despachos_DespachoId",
                        column: x => x.DespachoId,
                        principalTable: "Despachos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartesJuicio_Juicios_JuicioId",
                        column: x => x.JuicioId,
                        principalTable: "Juicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Plazos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DespachoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JuicioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaVencimiento = table.Column<DateOnly>(type: "date", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    DiasOriginales = table.Column<int>(type: "int", nullable: true),
                    DiasHabiles = table.Column<bool>(type: "bit", nullable: false),
                    ResponsableId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FechaCumplimiento = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NotasCumplimiento = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plazos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Plazos_AspNetUsers_ResponsableId",
                        column: x => x.ResponsableId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Plazos_Despachos_DespachoId",
                        column: x => x.DespachoId,
                        principalTable: "Despachos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Plazos_Juicios_JuicioId",
                        column: x => x.JuicioId,
                        principalTable: "Juicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Promociones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    DespachoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JuicioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    FechaPresentacion = table.Column<DateOnly>(type: "date", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Contenido = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    FirmanteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NumeroAcuse = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promociones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Promociones_AspNetUsers_FirmanteId",
                        column: x => x.FirmanteId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Promociones_Despachos_DespachoId",
                        column: x => x.DespachoId,
                        principalTable: "Despachos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Promociones_Juicios_JuicioId",
                        column: x => x.JuicioId,
                        principalTable: "Juicios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Actuaciones_DespachoId_JuicioId_Fecha",
                table: "Actuaciones",
                columns: new[] { "DespachoId", "JuicioId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_Actuaciones_JuicioId",
                table: "Actuaciones",
                column: "JuicioId");

            migrationBuilder.CreateIndex(
                name: "IX_Audiencias_AsignadoAId",
                table: "Audiencias",
                column: "AsignadoAId");

            migrationBuilder.CreateIndex(
                name: "IX_Audiencias_DespachoId_FechaHora",
                table: "Audiencias",
                columns: new[] { "DespachoId", "FechaHora" });

            migrationBuilder.CreateIndex(
                name: "IX_Audiencias_DespachoId_JuicioId",
                table: "Audiencias",
                columns: new[] { "DespachoId", "JuicioId" });

            migrationBuilder.CreateIndex(
                name: "IX_Audiencias_JuicioId",
                table: "Audiencias",
                column: "JuicioId");

            migrationBuilder.CreateIndex(
                name: "IX_Juicios_AsuntoId",
                table: "Juicios",
                column: "AsuntoId");

            migrationBuilder.CreateIndex(
                name: "IX_Juicios_DespachoId_AsuntoId",
                table: "Juicios",
                columns: new[] { "DespachoId", "AsuntoId" });

            migrationBuilder.CreateIndex(
                name: "IX_Juicios_DespachoId_Estado",
                table: "Juicios",
                columns: new[] { "DespachoId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_Juicios_DespachoId_NumeroExpediente",
                table: "Juicios",
                columns: new[] { "DespachoId", "NumeroExpediente" });

            migrationBuilder.CreateIndex(
                name: "IX_PartesJuicio_ClienteId",
                table: "PartesJuicio",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_PartesJuicio_DespachoId_JuicioId",
                table: "PartesJuicio",
                columns: new[] { "DespachoId", "JuicioId" });

            migrationBuilder.CreateIndex(
                name: "IX_PartesJuicio_JuicioId",
                table: "PartesJuicio",
                column: "JuicioId");

            migrationBuilder.CreateIndex(
                name: "IX_Plazos_DespachoId_Estado",
                table: "Plazos",
                columns: new[] { "DespachoId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_Plazos_DespachoId_FechaVencimiento",
                table: "Plazos",
                columns: new[] { "DespachoId", "FechaVencimiento" });

            migrationBuilder.CreateIndex(
                name: "IX_Plazos_JuicioId",
                table: "Plazos",
                column: "JuicioId");

            migrationBuilder.CreateIndex(
                name: "IX_Plazos_ResponsableId",
                table: "Plazos",
                column: "ResponsableId");

            migrationBuilder.CreateIndex(
                name: "IX_Promociones_DespachoId_JuicioId_FechaPresentacion",
                table: "Promociones",
                columns: new[] { "DespachoId", "JuicioId", "FechaPresentacion" });

            migrationBuilder.CreateIndex(
                name: "IX_Promociones_FirmanteId",
                table: "Promociones",
                column: "FirmanteId");

            migrationBuilder.CreateIndex(
                name: "IX_Promociones_JuicioId",
                table: "Promociones",
                column: "JuicioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Actuaciones");

            migrationBuilder.DropTable(
                name: "Audiencias");

            migrationBuilder.DropTable(
                name: "PartesJuicio");

            migrationBuilder.DropTable(
                name: "Plazos");

            migrationBuilder.DropTable(
                name: "Promociones");

            migrationBuilder.DropTable(
                name: "Juicios");
        }
    }
}
