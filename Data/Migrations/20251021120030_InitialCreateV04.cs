using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkOrderApplication.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateV04 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DestinationStationId",
                table: "ShipmentProcesses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OrderGroupId",
                table: "ShipmentProcesses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SourceStationId",
                table: "ShipmentProcesses",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DestinationStationId",
                table: "ShipmentProcesses");

            migrationBuilder.DropColumn(
                name: "OrderGroupId",
                table: "ShipmentProcesses");

            migrationBuilder.DropColumn(
                name: "SourceStationId",
                table: "ShipmentProcesses");
        }
    }
}
