using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JurisControl.Data.Migrations
{
    /// <inheritdoc />
    public partial class DocumentosAdjuntosAActuacionPromocion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Idempotente: el intento previo pudo dejar columnas, índices y hasta
            // la primera FK (a Actuaciones) creados antes de fallar en la de
            // Promociones. Verificamos con IF antes de cada CREATE.
            //
            // FIX importante: usamos NO ACTION en lugar de SET NULL. SQL Server
            // reclama multiple cascade paths (error 1785) porque Documentos ya
            // tiene FK con CASCADE a Cliente y Asunto, y ahora esas mismas
            // tablas serían alcanzables por otro camino vía Actuacion/Promocion.
            // Con NO ACTION cortamos el ciclo; al eliminar una Actuacion/Promocion
            // el Documento simplemente queda con FK huérfana (nullable, sin
            // impacto en la UI porque filtra por ActuacionId/PromocionId != null).

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'ActuacionId' AND Object_ID = Object_ID('dbo.Documentos'))
    ALTER TABLE dbo.Documentos ADD ActuacionId uniqueidentifier NULL;
");
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'PromocionId' AND Object_ID = Object_ID('dbo.Documentos'))
    ALTER TABLE dbo.Documentos ADD PromocionId uniqueidentifier NULL;
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Documentos_ActuacionId' AND object_id = OBJECT_ID('dbo.Documentos'))
    CREATE INDEX IX_Documentos_ActuacionId ON dbo.Documentos (ActuacionId);
");
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Documentos_PromocionId' AND object_id = OBJECT_ID('dbo.Documentos'))
    CREATE INDEX IX_Documentos_PromocionId ON dbo.Documentos (PromocionId);
");
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Documentos_DespachoId_ActuacionId' AND object_id = OBJECT_ID('dbo.Documentos'))
    CREATE INDEX IX_Documentos_DespachoId_ActuacionId ON dbo.Documentos (DespachoId, ActuacionId);
");
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Documentos_DespachoId_PromocionId' AND object_id = OBJECT_ID('dbo.Documentos'))
    CREATE INDEX IX_Documentos_DespachoId_PromocionId ON dbo.Documentos (DespachoId, PromocionId);
");

            // FK a Actuaciones — quitar si existe (puede haber quedado del intento
            // anterior con SET NULL) y recrear con NO ACTION.
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Documentos_Actuaciones_ActuacionId' AND parent_object_id = OBJECT_ID('dbo.Documentos'))
    ALTER TABLE dbo.Documentos DROP CONSTRAINT FK_Documentos_Actuaciones_ActuacionId;
");
            migrationBuilder.Sql(@"
ALTER TABLE dbo.Documentos
    ADD CONSTRAINT FK_Documentos_Actuaciones_ActuacionId
    FOREIGN KEY (ActuacionId) REFERENCES dbo.Actuaciones (Id)
    ON DELETE NO ACTION;
");

            // FK a Promociones — nunca llegó a crearse (aquí falló), pero por
            // seguridad limpiamos primero.
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Documentos_Promociones_PromocionId' AND parent_object_id = OBJECT_ID('dbo.Documentos'))
    ALTER TABLE dbo.Documentos DROP CONSTRAINT FK_Documentos_Promociones_PromocionId;
");
            migrationBuilder.Sql(@"
ALTER TABLE dbo.Documentos
    ADD CONSTRAINT FK_Documentos_Promociones_PromocionId
    FOREIGN KEY (PromocionId) REFERENCES dbo.Promociones (Id)
    ON DELETE NO ACTION;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documentos_Actuaciones_ActuacionId",
                table: "Documentos");

            migrationBuilder.DropForeignKey(
                name: "FK_Documentos_Promociones_PromocionId",
                table: "Documentos");

            migrationBuilder.DropIndex(
                name: "IX_Documentos_ActuacionId",
                table: "Documentos");

            migrationBuilder.DropIndex(
                name: "IX_Documentos_DespachoId_ActuacionId",
                table: "Documentos");

            migrationBuilder.DropIndex(
                name: "IX_Documentos_DespachoId_PromocionId",
                table: "Documentos");

            migrationBuilder.DropIndex(
                name: "IX_Documentos_PromocionId",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "ActuacionId",
                table: "Documentos");

            migrationBuilder.DropColumn(
                name: "PromocionId",
                table: "Documentos");
        }
    }
}
