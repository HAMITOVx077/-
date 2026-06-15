using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace МаршрутСборки.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Components",
                columns: table => new
                {
                    ComponentId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SKU = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    StockBalance = table.Column<int>(type: "integer", nullable: false),
                    MinStock = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Components", x => x.ComponentId);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    Login = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    RoleId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Assemblies",
                columns: table => new
                {
                    AssemblyId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AssemblyNumber = table.Column<string>(type: "text", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ClientName = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    DispatcherId = table.Column<int>(type: "integer", nullable: false),
                    AssemblerId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assemblies", x => x.AssemblyId);
                    table.ForeignKey(
                        name: "FK_Assemblies_Users_AssemblerId",
                        column: x => x.AssemblerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Assemblies_Users_DispatcherId",
                        column: x => x.DispatcherId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EventLogs",
                columns: table => new
                {
                    LogId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ActionType = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ActionTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventLogs", x => x.LogId);
                    table.ForeignKey(
                        name: "FK_EventLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssemblyComponents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AssemblyId = table.Column<int>(type: "integer", nullable: false),
                    ComponentId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssemblyComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssemblyComponents_Assemblies_AssemblyId",
                        column: x => x.AssemblyId,
                        principalTable: "Assemblies",
                        principalColumn: "AssemblyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssemblyComponents_Components_ComponentId",
                        column: x => x.ComponentId,
                        principalTable: "Components",
                        principalColumn: "ComponentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tests",
                columns: table => new
                {
                    TestId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Result = table.Column<string>(type: "text", nullable: false),
                    Defects = table.Column<string>(type: "text", nullable: true),
                    TestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    AssemblyId = table.Column<int>(type: "integer", nullable: false),
                    TesterId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tests", x => x.TestId);
                    table.ForeignKey(
                        name: "FK_Tests_Assemblies_AssemblyId",
                        column: x => x.AssemblyId,
                        principalTable: "Assemblies",
                        principalColumn: "AssemblyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tests_Users_TesterId",
                        column: x => x.TesterId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WarehouseOperations",
                columns: table => new
                {
                    OperationId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OperationType = table.Column<string>(type: "text", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    OperationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DocumentRef = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    ComponentId = table.Column<int>(type: "integer", nullable: false),
                    AssemblyId = table.Column<int>(type: "integer", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarehouseOperations", x => x.OperationId);
                    table.ForeignKey(
                        name: "FK_WarehouseOperations_Assemblies_AssemblyId",
                        column: x => x.AssemblyId,
                        principalTable: "Assemblies",
                        principalColumn: "AssemblyId");
                    table.ForeignKey(
                        name: "FK_WarehouseOperations_Components_ComponentId",
                        column: x => x.ComponentId,
                        principalTable: "Components",
                        principalColumn: "ComponentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WarehouseOperations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WarrantyCases",
                columns: table => new
                {
                    CaseId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CaseNumber = table.Column<string>(type: "text", nullable: false),
                    ClientName = table.Column<string>(type: "text", nullable: false),
                    ClientPhone = table.Column<string>(type: "text", nullable: false),
                    ProblemDescription = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ReceivedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClosedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RepairNotes = table.Column<string>(type: "text", nullable: true),
                    AssemblyId = table.Column<int>(type: "integer", nullable: true),
                    EngineerId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarrantyCases", x => x.CaseId);
                    table.ForeignKey(
                        name: "FK_WarrantyCases_Assemblies_AssemblyId",
                        column: x => x.AssemblyId,
                        principalTable: "Assemblies",
                        principalColumn: "AssemblyId");
                    table.ForeignKey(
                        name: "FK_WarrantyCases_Users_EngineerId",
                        column: x => x.EngineerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Components",
                columns: new[] { "ComponentId", "Category", "MinStock", "Name", "Price", "SKU", "StockBalance" },
                values: new object[,]
                {
                    { 1, "Процессор", 3, "Процессор Intel Core i3-12100", 12500m, "CPU-I3-12100", 15 },
                    { 2, "Процессор", 3, "Процессор Intel Core i5-12400", 18900m, "CPU-I5-12400", 10 },
                    { 3, "Процессор", 2, "Процессор Intel Core i5-14400", 22500m, "CPU-I5-14400", 8 },
                    { 4, "Процессор", 2, "Процессор Intel Core i7-13700", 31000m, "CPU-I7-13700", 5 },
                    { 5, "Оперативная память", 5, "ОЗУ 8 GB DDR5", 4200m, "RAM-8GB-DDR5", 30 },
                    { 6, "Оперативная память", 5, "ОЗУ 16 GB DDR5", 7800m, "RAM-16GB-DDR5", 25 },
                    { 7, "Оперативная память", 3, "ОЗУ 32 GB DDR5", 14500m, "RAM-32GB-DDR5", 12 },
                    { 8, "Накопитель", 10, "SSD накопитель 512 GB", 5500m, "SSD-512GB", 40 },
                    { 9, "Накопитель", 5, "SSD накопитель 1 TB", 9200m, "SSD-1TB", 20 },
                    { 10, "Видеокарта", 2, "Видеокарта RTX 3050 8GB", 24000m, "GPU-RTX3050", 6 },
                    { 11, "Видеокарта", 2, "Видеокарта RTX 5060 8GB", 38000m, "GPU-RTX5060", 4 },
                    { 12, "Блок питания", 4, "Блок питания 600W APFC", 6800m, "PSU-600W", 18 },
                    { 13, "Блок питания", 3, "Блок питания 700W APFC", 8500m, "PSU-700W", 10 },
                    { 14, "Корпус", 5, "Корпус NITRINOnet", 4500m, "CASE-NITRINO", 22 },
                    { 15, "Материнская плата", 2, "Материнская плата ASUS Z690", 18200m, "MB-Z690", 8 }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "RoleName" },
                values: new object[,]
                {
                    { 1, "Администратор" },
                    { 2, "Диспетчер" },
                    { 3, "Сборщик" },
                    { 4, "Кладовщик" },
                    { 5, "Тестировщик" },
                    { 6, "Гарантийный инженер" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "FirstName", "IsActive", "LastName", "Login", "PasswordHash", "RoleId" },
                values: new object[] { 1, "Системный", true, "Администратор", "admin", "$2a$11$nBagiv87YrcYYMm07aplpOnTK9MmRji86nXQBmPyljUoF/QSLuqbG", 1 });

            migrationBuilder.CreateIndex(
                name: "IX_Assemblies_AssemblerId",
                table: "Assemblies",
                column: "AssemblerId");

            migrationBuilder.CreateIndex(
                name: "IX_Assemblies_DispatcherId",
                table: "Assemblies",
                column: "DispatcherId");

            migrationBuilder.CreateIndex(
                name: "IX_AssemblyComponents_AssemblyId_ComponentId",
                table: "AssemblyComponents",
                columns: new[] { "AssemblyId", "ComponentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssemblyComponents_ComponentId",
                table: "AssemblyComponents",
                column: "ComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_EventLogs_UserId",
                table: "EventLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tests_AssemblyId",
                table: "Tests",
                column: "AssemblyId");

            migrationBuilder.CreateIndex(
                name: "IX_Tests_TesterId",
                table: "Tests",
                column: "TesterId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseOperations_AssemblyId",
                table: "WarehouseOperations",
                column: "AssemblyId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseOperations_ComponentId",
                table: "WarehouseOperations",
                column: "ComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseOperations_UserId",
                table: "WarehouseOperations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyCases_AssemblyId",
                table: "WarrantyCases",
                column: "AssemblyId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyCases_EngineerId",
                table: "WarrantyCases",
                column: "EngineerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssemblyComponents");

            migrationBuilder.DropTable(
                name: "EventLogs");

            migrationBuilder.DropTable(
                name: "Tests");

            migrationBuilder.DropTable(
                name: "WarehouseOperations");

            migrationBuilder.DropTable(
                name: "WarrantyCases");

            migrationBuilder.DropTable(
                name: "Components");

            migrationBuilder.DropTable(
                name: "Assemblies");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
