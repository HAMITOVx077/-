using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace МаршрутСборки.Migrations
{
    public partial class AddReworkItems : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssemblyReworkItems",
                columns: table => new
                {
                    ReworkItemId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AssemblyId = table.Column<int>(type: "integer", nullable: false),
                    OldComponentId = table.Column<int>(type: "integer", nullable: true),
                    NewComponentId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    IsIssued = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssemblyReworkItems", x => x.ReworkItemId);
                    table.ForeignKey(
                        name: "FK_AssemblyReworkItems_Assemblies_AssemblyId",
                        column: x => x.AssemblyId,
                        principalTable: "Assemblies",
                        principalColumn: "AssemblyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssemblyReworkItems_Components_OldComponentId",
                        column: x => x.OldComponentId,
                        principalTable: "Components",
                        principalColumn: "ComponentId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AssemblyReworkItems_Components_NewComponentId",
                        column: x => x.NewComponentId,
                        principalTable: "Components",
                        principalColumn: "ComponentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssemblyReworkItems_AssemblyId",
                table: "AssemblyReworkItems",
                column: "AssemblyId");

            migrationBuilder.CreateIndex(
                name: "IX_AssemblyReworkItems_NewComponentId",
                table: "AssemblyReworkItems",
                column: "NewComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssemblyReworkItems_OldComponentId",
                table: "AssemblyReworkItems",
                column: "OldComponentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AssemblyReworkItems");
        }
    }
}
