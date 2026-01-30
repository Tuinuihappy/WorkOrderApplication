using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkOrderApplication.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateV02 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderMaterials_Materials_MaterialId",
                table: "OrderMaterials");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderMaterials_Materials_MaterialId",
                table: "OrderMaterials",
                column: "MaterialId",
                principalTable: "Materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderMaterials_Materials_MaterialId",
                table: "OrderMaterials");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderMaterials_Materials_MaterialId",
                table: "OrderMaterials",
                column: "MaterialId",
                principalTable: "Materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
