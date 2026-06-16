using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace МаршрутСборки.Migrations
{
    /// <inheritdoc />
    public partial class FixRoleNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 1,
                column: "RoleName",
                value: "Технический директор");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 6,
                column: "RoleName",
                value: "Инженер сервисного центра");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$W3GKMUD6dPmHQdTRg56lluqkexVEx/Mq9X/B7I5rII6hvFhM5iCgi");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 2,
                column: "PasswordHash",
                value: "$2a$11$ygD2TVYfkHqahZQMDwI.UehVaobzJe/tUDMzhoCRLrILh82Fs6MSO");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 3,
                column: "PasswordHash",
                value: "$2a$11$y1K1C7VxJLSbHyCHgf433uQqqYz5lajOzXwslm5c0.2n77xxvAJJa");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 4,
                column: "PasswordHash",
                value: "$2a$11$4a2gJZZyVL/doLG9fljLNePw1mDOjTW0WEFbX3uNLkoYY/XWW0gVK");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 5,
                column: "PasswordHash",
                value: "$2a$11$pCtwQl5XQMCBUyrz9O3eOeE2Z5Nki6J6jS6OEysctVejHxQa8JM0C");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 6,
                column: "PasswordHash",
                value: "$2a$11$h8w8W2XLME4/WBt7bLw.B.05qqe6yxZMNASigE.nr4VYihlEZRGhO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 1,
                column: "RoleName",
                value: "Администратор");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "RoleId",
                keyValue: 6,
                column: "RoleName",
                value: "Гарантийный инженер");

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
    }
}
