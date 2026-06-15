using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace МаршрутСборки.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigurationToAssembly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Configuration",
                table: "Assemblies",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$vZKqrn4feRgPHcjqtYxlY.MSDYCZ.pcpKYUbrrumJUJnYnNoyNHWC");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Configuration",
                table: "Assemblies");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$nBagiv87YrcYYMm07aplpOnTK9MmRji86nXQBmPyljUoF/QSLuqbG");
        }
    }
}
