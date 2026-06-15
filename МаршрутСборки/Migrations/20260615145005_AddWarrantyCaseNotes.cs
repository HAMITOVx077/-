using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace МаршрутСборки.Migrations
{
    /// <inheritdoc />
    public partial class AddWarrantyCaseNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WarrantyCaseNotes",
                columns: table => new
                {
                    NoteId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CaseId = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AuthorId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarrantyCaseNotes", x => x.NoteId);
                    table.ForeignKey(
                        name: "FK_WarrantyCaseNotes_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarrantyCaseNotes_WarrantyCases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "WarrantyCases",
                        principalColumn: "CaseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$YPIT8L5TJiGtpMzpsQJMVudZpd9LA.GAk0n.719LFbjhwAN3Wrcba");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$.4.Edg7aoKAUE5v4L5XFJOuoN9QkJyDZ5o3V/dRGDkgzU9zr.0ipa");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$ngrf2nmtaX902qX9Ano1EOWwNc4dmR/9N6.B9lqRS39/gyE7wJbzm");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 4,
                column: "PasswordHash",
                value: "$2a$11$2jlLRIhST5Y6v0LJFwbn0.GicEZcmN3mchJpyYfkGGG/nRYSvPoyu");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5,
                column: "PasswordHash",
                value: "$2a$11$zVpVDFZUAEf.VH8YDwhCau/AmgG4IOZUIBq5q7STMhxUsoP8w95dK");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 6,
                column: "PasswordHash",
                value: "$2a$11$2S0UjCmVdZWaqmcoa7e4DeVt1RZOo1Wi3h3wIcXQZdDDETFEc39D6");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyCaseNotes_AuthorId",
                table: "WarrantyCaseNotes",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_WarrantyCaseNotes_CaseId",
                table: "WarrantyCaseNotes",
                column: "CaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WarrantyCaseNotes");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$hrlxsMJB.NEwFgw6fyyNS.OpfRclYVCGmGysR/v5wl7/vCtledo8.");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$plLvMXaTS.hR4HQUBQ8oGeVBxfCUIDfE9nHVKz3SyY/hew0oCAnqm");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$X8V5xGfNnNvSU7BgxUDQe.NSP8VJiTD0.wmanm/0aY5k9ZUeBceXC");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 4,
                column: "PasswordHash",
                value: "$2a$11$sMXxqswaR.9kzF4Oq9rdf.DHHX7T1.hwyX6IxeQs5uGvVMAiRBEbi");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5,
                column: "PasswordHash",
                value: "$2a$11$gHzc..g5As5GkVR5/adOPO4RPA.lNIXf0Qwz/pPGhZ8anKwv2FGDS");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 6,
                column: "PasswordHash",
                value: "$2a$11$W4guvkqdcUcqZXqxUTrkXuv7XQQXR.xw7eYEiGw4vUXy/LQAz5Z5m");
        }
    }
}
