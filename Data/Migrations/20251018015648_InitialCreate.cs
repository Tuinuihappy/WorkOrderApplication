using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WorkOrderApplication.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderGroupAMRs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SourceStationId = table.Column<int>(type: "integer", nullable: false),
                    SourceStation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DestinationStationId = table.Column<int>(type: "integer", nullable: false),
                    DestinationStation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OrderGroupId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderGroupAMRs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderRecordByIds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    OrderId = table.Column<string>(type: "text", nullable: false),
                    OrderName = table.Column<string>(type: "text", nullable: false),
                    OrderState = table.Column<int>(type: "integer", nullable: false),
                    OrderType = table.Column<int>(type: "integer", nullable: false),
                    Progress = table.Column<double>(type: "double precision", nullable: false),
                    ExecutingIndex = table.Column<int>(type: "integer", nullable: false),
                    StartStationName = table.Column<string>(type: "text", nullable: false),
                    StartStationNo = table.Column<int>(type: "integer", nullable: false),
                    EndStationName = table.Column<string>(type: "text", nullable: false),
                    EndStationNo = table.Column<int>(type: "integer", nullable: false),
                    ExecuteVehicleName = table.Column<string>(type: "text", nullable: false),
                    ExecuteVehicleKey = table.Column<string>(type: "text", nullable: false),
                    TaskState = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    FailReason = table.Column<string>(type: "text", nullable: false),
                    StartEndStationNameDetail = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedBy = table.Column<string>(type: "text", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DoneTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RawResponse = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderRecordByIds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    OrderName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    ExecutingIndex = table.Column<int>(type: "integer", nullable: false),
                    Progress = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP AT TIME ZONE 'UTC'"),
                    RawResponse = table.Column<string>(type: "jsonb", nullable: true),
                    Source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EmployeeId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Position = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Shift = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ContactNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderMissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderRecordByIdId = table.Column<int>(type: "integer", nullable: false),
                    MissionState = table.Column<int>(type: "integer", nullable: false),
                    ExecutingIndex = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    ActionName = table.Column<string>(type: "text", nullable: false),
                    DestinationName = table.Column<string>(type: "text", nullable: false),
                    MapName = table.Column<string>(type: "text", nullable: false),
                    ResultCode = table.Column<int>(type: "integer", nullable: false),
                    ResultStr = table.Column<string>(type: "text", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExecuteTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FinishTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderMissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderMissions_OrderRecordByIds_OrderRecordByIdId",
                        column: x => x.OrderRecordByIdId,
                        principalTable: "OrderRecordByIds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkOrderNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LineName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ModelName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkOrders_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkOrders_Users_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Materials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MaterialNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    RequestPerHour = table.Column<int>(type: "integer", nullable: false),
                    Unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    WorkOrderId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Materials_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderProcesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    TimeToUse = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    WorkOrderId = table.Column<int>(type: "integer", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderProcesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderProcesses_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderProcesses_WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CancelledProcesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CancelledDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CancelledByUserId = table.Column<int>(type: "integer", nullable: true),
                    OrderProcessId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CancelledProcesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CancelledProcesses_OrderProcesses_OrderProcessId",
                        column: x => x.OrderProcessId,
                        principalTable: "OrderProcesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CancelledProcesses_Users_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ConfirmProcesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderProcessId = table.Column<int>(type: "integer", nullable: false),
                    ConfirmedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfirmProcesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfirmProcesses_OrderProcesses_OrderProcessId",
                        column: x => x.OrderProcessId,
                        principalTable: "OrderProcesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderMaterials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderProcessId = table.Column<int>(type: "integer", nullable: false),
                    MaterialId = table.Column<int>(type: "integer", nullable: false),
                    OrderQty = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderMaterials_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderMaterials_OrderProcesses_OrderProcessId",
                        column: x => x.OrderProcessId,
                        principalTable: "OrderProcesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PreparingProcesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PreparedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    PreparingByUserId = table.Column<int>(type: "integer", nullable: false),
                    OrderProcessId = table.Column<int>(type: "integer", nullable: false),
                    ShortageReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreparingProcesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreparingProcesses_OrderProcesses_OrderProcessId",
                        column: x => x.OrderProcessId,
                        principalTable: "OrderProcesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PreparingProcesses_Users_PreparingByUserId",
                        column: x => x.PreparingByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReceivedProcesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReceivedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ReceivedByUserId = table.Column<int>(type: "integer", nullable: false),
                    ShortageReason = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    OrderProcessId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceivedProcesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceivedProcesses_OrderProcesses_OrderProcessId",
                        column: x => x.OrderProcessId,
                        principalTable: "OrderProcesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReceivedProcesses_Users_ReceivedByUserId",
                        column: x => x.ReceivedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReturnProcesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReturnDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ReturnByUserId = table.Column<int>(type: "integer", nullable: false),
                    OrderProcessId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnProcesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReturnProcesses_OrderProcesses_OrderProcessId",
                        column: x => x.OrderProcessId,
                        principalTable: "OrderProcesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReturnProcesses_Users_ReturnByUserId",
                        column: x => x.ReturnByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShipmentProcesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SourceStation = table.Column<string>(type: "text", nullable: false),
                    DestinationStation = table.Column<string>(type: "text", nullable: false),
                    ExternalId = table.Column<int>(type: "integer", nullable: false),
                    OrderId = table.Column<string>(type: "text", nullable: false),
                    OrderName = table.Column<string>(type: "text", nullable: false),
                    ArrivalTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    OrderState = table.Column<int>(type: "integer", nullable: true),
                    ExecutingIndex = table.Column<int>(type: "integer", nullable: true),
                    Progress = table.Column<double>(type: "double precision", nullable: true),
                    ExecuteVehicleName = table.Column<string>(type: "text", nullable: true),
                    ExecuteVehicleKey = table.Column<string>(type: "text", nullable: true),
                    LastSynced = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    OrderProcessId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentProcesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShipmentProcesses_OrderProcesses_OrderProcessId",
                        column: x => x.OrderProcessId,
                        principalTable: "OrderProcesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PreparingMaterials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PreparingProcessId = table.Column<int>(type: "integer", nullable: false),
                    MaterialId = table.Column<int>(type: "integer", nullable: false),
                    PreparedQty = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreparingMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreparingMaterials_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PreparingMaterials_PreparingProcesses_PreparingProcessId",
                        column: x => x.PreparingProcessId,
                        principalTable: "PreparingProcesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReceivedMaterials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReceivedProcessId = table.Column<int>(type: "integer", nullable: false),
                    MaterialId = table.Column<int>(type: "integer", nullable: false),
                    ReceivedQty = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceivedMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceivedMaterials_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReceivedMaterials_ReceivedProcesses_ReceivedProcessId",
                        column: x => x.ReceivedProcessId,
                        principalTable: "ReceivedProcesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CancelledProcesses_CancelledByUserId",
                table: "CancelledProcesses",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CancelledProcesses_OrderProcessId",
                table: "CancelledProcesses",
                column: "OrderProcessId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConfirmProcesses_OrderProcessId",
                table: "ConfirmProcesses",
                column: "OrderProcessId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Materials_WorkOrderId_MaterialNumber",
                table: "Materials",
                columns: new[] { "WorkOrderId", "MaterialNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderGroupAMRs_OrderGroupId",
                table: "OrderGroupAMRs",
                column: "OrderGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderGroupAMRs_SourceStationId_DestinationStationId",
                table: "OrderGroupAMRs",
                columns: new[] { "SourceStationId", "DestinationStationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderMaterials_MaterialId",
                table: "OrderMaterials",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderMaterials_OrderProcessId",
                table: "OrderMaterials",
                column: "OrderProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderMissions_OrderRecordByIdId",
                table: "OrderMissions",
                column: "OrderRecordByIdId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderProcesses_CreatedByUserId",
                table: "OrderProcesses",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderProcesses_OrderNumber",
                table: "OrderProcesses",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderProcesses_WorkOrderId",
                table: "OrderProcesses",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PreparingMaterials_MaterialId",
                table: "PreparingMaterials",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_PreparingMaterials_PreparingProcessId",
                table: "PreparingMaterials",
                column: "PreparingProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_PreparingProcesses_OrderProcessId",
                table: "PreparingProcesses",
                column: "OrderProcessId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreparingProcesses_PreparingByUserId",
                table: "PreparingProcesses",
                column: "PreparingByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivedMaterials_MaterialId",
                table: "ReceivedMaterials",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivedMaterials_ReceivedProcessId",
                table: "ReceivedMaterials",
                column: "ReceivedProcessId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivedProcesses_OrderProcessId",
                table: "ReceivedProcesses",
                column: "OrderProcessId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReceivedProcesses_ReceivedByUserId",
                table: "ReceivedProcesses",
                column: "ReceivedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnProcesses_OrderProcessId",
                table: "ReturnProcesses",
                column: "OrderProcessId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReturnProcesses_ReturnByUserId",
                table: "ReturnProcesses",
                column: "ReturnByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentProcesses_OrderProcessId",
                table: "ShipmentProcesses",
                column: "OrderProcessId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmployeeId",
                table: "Users",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_CreatedByUserId",
                table: "WorkOrders",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_UpdatedByUserId",
                table: "WorkOrders",
                column: "UpdatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrders_WorkOrderNumber",
                table: "WorkOrders",
                column: "WorkOrderNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CancelledProcesses");

            migrationBuilder.DropTable(
                name: "ConfirmProcesses");

            migrationBuilder.DropTable(
                name: "OrderGroupAMRs");

            migrationBuilder.DropTable(
                name: "OrderMaterials");

            migrationBuilder.DropTable(
                name: "OrderMissions");

            migrationBuilder.DropTable(
                name: "OrderRecords");

            migrationBuilder.DropTable(
                name: "PreparingMaterials");

            migrationBuilder.DropTable(
                name: "ReceivedMaterials");

            migrationBuilder.DropTable(
                name: "ReturnProcesses");

            migrationBuilder.DropTable(
                name: "ShipmentProcesses");

            migrationBuilder.DropTable(
                name: "OrderRecordByIds");

            migrationBuilder.DropTable(
                name: "PreparingProcesses");

            migrationBuilder.DropTable(
                name: "Materials");

            migrationBuilder.DropTable(
                name: "ReceivedProcesses");

            migrationBuilder.DropTable(
                name: "OrderProcesses");

            migrationBuilder.DropTable(
                name: "WorkOrders");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
