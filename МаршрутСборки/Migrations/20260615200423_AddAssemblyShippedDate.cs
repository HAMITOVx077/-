using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace МаршрутСборки.Migrations
{
    /// <inheritdoc />
    public partial class AddAssemblyShippedDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ShippedDate",
                table: "Assemblies",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$9g1kDJ2.cg3jsIOUbMif6eJT5XldVmhavbM4XL96xwGzUc48FlP46");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$7pHooEzSreJMLtBtt8mJPOWi20fIZm.64pDbHaAmYAQKOJNRrcdru");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$DBaLoqjmbMeUJW046Hyfy.gcPWO9Sz8AIIBWJK.swldbMYifVrRJm");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 4,
                column: "PasswordHash",
                value: "$2a$11$l2f9V2k2HLYFkV1rpYXNye0H6XcMrJQk6M/kDr1EmkTypwy6ZDzZK");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5,
                column: "PasswordHash",
                value: "$2a$11$2Ic0OhDv8Ad2CgkGMWsYIuM3zPIEd9IIdhuFWhBAciQkYMs.hkSvW");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 6,
                column: "PasswordHash",
                value: "$2a$11$pVna6URry9fOwAXwdrtDl.WRvanlmMD3DGFkDEzFyMCGtWdjRkxsK");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShippedDate",
                table: "Assemblies");

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
        }
    }
}
