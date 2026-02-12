using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkOrderApplication.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameMaterialProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrders_Users_CreatedByUserId",
                table: "WorkOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrders_Users_UpdatedByUserId",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_CreatedByUserId",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_UpdatedByUserId",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "LineName",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "ModelName",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "RequestPerHour",
                table: "Materials");

            migrationBuilder.RenameColumn(
                name: "WorkOrderNumber",
                table: "WorkOrders",
                newName: "Order");

            migrationBuilder.RenameIndex(
                name: "IX_WorkOrders_WorkOrderNumber",
                table: "WorkOrders",
                newName: "IX_WorkOrders_Order");

            migrationBuilder.RenameColumn(
                name: "Unit",
                table: "Materials",
                newName: "BUn");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Materials",
                newName: "MaterialDescription");

            migrationBuilder.AlterColumn<int>(
                name: "CreatedByUserId",
                table: "WorkOrders",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<DateTime>(
                name: "BasicFinishDate",
                table: "WorkOrders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedById",
                table: "WorkOrders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Material",
                table: "WorkOrders",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OrderType",
                table: "WorkOrders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Plant",
                table: "WorkOrders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "WorkOrders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "PCE");

            migrationBuilder.AddColumn<int>(
                name: "UpdatedById",
                table: "WorkOrders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OpAc",
                table: "Materials",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "QtyWthdrn",
                table: "Materials",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ReqmntQty",
                table: "Materials",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "SLoc",
                table: "Materials",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SortString",
                table: "Materials",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_CreatedById",
                table: "WorkOrders",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_UpdatedById",
                table: "WorkOrders",
                column: "UpdatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrders_Users_CreatedById",
                table: "WorkOrders",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrders_Users_UpdatedById",
                table: "WorkOrders",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrders_Users_CreatedById",
                table: "WorkOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkOrders_Users_UpdatedById",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_CreatedById",
                table: "WorkOrders");

            migrationBuilder.DropIndex(
                name: "IX_WorkOrders_UpdatedById",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "BasicFinishDate",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "Material",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "OrderType",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "Plant",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "OpAc",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "QtyWthdrn",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "ReqmntQty",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "SLoc",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "SortString",
                table: "Materials");

            migrationBuilder.RenameColumn(
                name: "Order",
                table: "WorkOrders",
                newName: "WorkOrderNumber");

            migrationBuilder.RenameIndex(
                name: "IX_WorkOrders_Order",
                table: "WorkOrders",
                newName: "IX_WorkOrders_WorkOrderNumber");

            migrationBuilder.RenameColumn(
                name: "MaterialDescription",
                table: "Materials",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "BUn",
                table: "Materials",
                newName: "Unit");

            migrationBuilder.AlterColumn<int>(
                name: "CreatedByUserId",
                table: "WorkOrders",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LineName",
                table: "WorkOrders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModelName",
                table: "WorkOrders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Materials",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RequestPerHour",
                table: "Materials",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_CreatedByUserId",
                table: "WorkOrders",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_UpdatedByUserId",
                table: "WorkOrders",
                column: "UpdatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrders_Users_CreatedByUserId",
                table: "WorkOrders",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkOrders_Users_UpdatedByUserId",
                table: "WorkOrders",
                column: "UpdatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
