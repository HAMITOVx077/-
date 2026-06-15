using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace МаршрутСборки.Migrations
{
    /// <inheritdoc />
    public partial class AddTestUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$hrlxsMJB.NEwFgw6fyyNS.OpfRclYVCGmGysR/v5wl7/vCtledo8.");

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "FirstName", "IsActive", "LastName", "Login", "PasswordHash", "RoleId" },
                values: new object[,]
                {
                    { 2, "Иван", true, "Иванов", "ivanov", "$2a$11$plLvMXaTS.hR4HQUBQ8oGeVBxfCUIDfE9nHVKz3SyY/hew0oCAnqm", 3 },
                    { 3, "Пётр", true, "Петров", "petrov", "$2a$11$X8V5xGfNnNvSU7BgxUDQe.NSP8VJiTD0.wmanm/0aY5k9ZUeBceXC", 5 },
                    { 4, "Алексей", true, "Сидоров", "sidorov", "$2a$11$sMXxqswaR.9kzF4Oq9rdf.DHHX7T1.hwyX6IxeQs5uGvVMAiRBEbi", 4 },
                    { 5, "Михаил", true, "Фёдоров", "fedorov", "$2a$11$gHzc..g5As5GkVR5/adOPO4RPA.lNIXf0Qwz/pPGhZ8anKwv2FGDS", 2 },
                    { 6, "Сергей", true, "Алексеев", "alekseev", "$2a$11$W4guvkqdcUcqZXqxUTrkXuv7XQQXR.xw7eYEiGw4vUXy/LQAz5Z5m", 6 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 6);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$vZKqrn4feRgPHcjqtYxlY.MSDYCZ.pcpKYUbrrumJUJnYnNoyNHWC");
        }
    }
}
