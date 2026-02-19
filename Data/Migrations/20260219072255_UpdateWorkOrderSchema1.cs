using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkOrderApplication.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWorkOrderSchema1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_CreatedDate",
                table: "WorkOrders",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_Material",
                table: "WorkOrders",
                column: "Material");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_OrderType",
                table: "WorkOrders",
                column: "OrderType");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_Plant",
                table: "WorkOrders",
                column: "Plant");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_CreatedDate",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_Material",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_OrderType",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_Plant",
                table: "WorkOrders");
        }
    }
}
