using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkOrderApplication.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeOrderProcessQueries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ShipmentProcesses_DestinationStation",
                table: "ShipmentProcesses",
                column: "DestinationStation");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentProcesses_SourceStation",
                table: "ShipmentProcesses",
                column: "SourceStation");

            migrationBuilder.CreateIndex(
                name: "IX_OrderProcesses_CreatedDate",
                table: "OrderProcesses",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_OrderProcesses_Status",
                table: "OrderProcesses",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ShipmentProcesses_DestinationStation",
                table: "ShipmentProcesses");

            migrationBuilder.DropIndex(
                name: "IX_ShipmentProcesses_SourceStation",
                table: "ShipmentProcesses");

            migrationBuilder.DropIndex(
                name: "IX_OrderProcesses_CreatedDate",
                table: "OrderProcesses");

            migrationBuilder.DropIndex(
                name: "IX_OrderProcesses_Status",
                table: "OrderProcesses");
        }
    }
}
