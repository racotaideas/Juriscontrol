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
            migrationBuilder.AddColumn<Guid>(
                name: "ActuacionId",
                table: "Documentos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PromocionId",
                table: "Documentos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_ActuacionId",
                table: "Documentos",
                column: "ActuacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_DespachoId_ActuacionId",
                table: "Documentos",
                columns: new[] { "DespachoId", "ActuacionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_DespachoId_PromocionId",
                table: "Documentos",
                columns: new[] { "DespachoId", "PromocionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_PromocionId",
                table: "Documentos",
                column: "PromocionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Documentos_Actuaciones_ActuacionId",
                table: "Documentos",
                column: "ActuacionId",
                principalTable: "Actuaciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Documentos_Promociones_PromocionId",
                table: "Documentos",
                column: "PromocionId",
                principalTable: "Promociones",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
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
