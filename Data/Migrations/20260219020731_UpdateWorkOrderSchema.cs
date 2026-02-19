using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkOrderApplication.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWorkOrderSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultLine",
                table: "WorkOrders",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultLine",
                table: "WorkOrders");
        }
    }
}
