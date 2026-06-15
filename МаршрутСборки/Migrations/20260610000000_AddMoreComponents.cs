using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814

namespace МаршрутСборки.Migrations
{
    public partial class AddMoreComponents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Components",
                columns: new[] { "ComponentId", "Category", "MinStock", "Name", "Price", "SKU", "StockBalance" },
                values: new object[,]
                {
                    { 16, "Корпус",    3, "Корпус NITRINOnet S600 (системный блок)", 8900m,  "CASE-S600",     15 },
                    { 17, "Корпус",    2, "Корпус NITRINOnet S600M (моноблок)",       14500m, "CASE-S600M",    8  },
                    { 18, "Монитор",   2, "Монитор 22\" Full HD",                     18850m, "MON-22",        10 },
                    { 19, "Периферия", 5, "Клавиатура НЬЮКЛИК (PREMIUM)",             2800m,  "KB-NEWCLICK",   20 },
                    { 20, "Периферия", 5, "Мышь НЬЮКЛИК (PREMIUM)",                  1950m,  "MS-NEWCLICK",   0  }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            for (int id = 16; id <= 20; id++)
                migrationBuilder.DeleteData(table: "Components", keyColumn: "ComponentId", keyValue: id);
        }
    }
}
