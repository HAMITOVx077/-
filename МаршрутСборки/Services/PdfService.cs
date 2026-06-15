using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using МаршрутСборки.Models;

namespace МаршрутСборки.Services
{
    public class PdfService
    {
        public PdfService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        // Акт выдачи комплектующих
        public byte[] GenerateIssueAct(Models.Assembly assembly, User issuedBy)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Content().Column(col =>
                    {
                        // Шапка
                        col.Item().AlignCenter().Text("ООО «Нью Ай Ти»")
                            .FontSize(14).Bold();
                        col.Item().AlignCenter().Text("АКТ ВЫДАЧИ КОМПЛЕКТУЮЩИХ")
                            .FontSize(16).Bold();
                        col.Item().AlignCenter().Text($"№ {assembly.AssemblyNumber}")
                            .FontSize(12);
                        col.Item().AlignCenter().Text($"от {DateTime.Now:dd.MM.yyyy}")
                            .FontSize(11);

                        col.Item().PaddingVertical(10).LineHorizontal(1);

                        // Информация о сборке
                        col.Item().PaddingBottom(6).Text(text =>
                        {
                            text.Span("Заказ: ").Bold();
                            text.Span(assembly.AssemblyNumber);
                        });
                        col.Item().PaddingBottom(6).Text(text =>
                        {
                            text.Span("Клиент: ").Bold();
                            text.Span(assembly.ClientName);
                        });
                        col.Item().PaddingBottom(6).Text(text =>
                        {
                            text.Span("Сборщик: ").Bold();
                            text.Span(assembly.Assembler?.FullName ?? "—");
                        });
                        col.Item().PaddingBottom(6).Text(text =>
                        {
                            text.Span("Диспетчер: ").Bold();
                            text.Span(assembly.Dispatcher?.FullName ?? "—");
                        });
                        col.Item().PaddingBottom(14).Text(text =>
                        {
                            text.Span("Срок выполнения: ").Bold();
                            text.Span(assembly.Deadline?.ToString("dd.MM.yyyy") ?? "—");
                        });

                        // Таблица комплектующих
                        col.Item().PaddingBottom(8).Text("Перечень выдаваемых комплектующих:")
                            .Bold().FontSize(12);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);
                                columns.RelativeColumn(4);
                                columns.RelativeColumn(2);
                                columns.ConstantColumn(60);
                                columns.RelativeColumn(2);
                            });

                            // Заголовок
                            table.Header(header =>
                            {
                                header.Cell().Background("#1E293B").Padding(6)
                                    .Text("№").FontColor("#FFFFFF").Bold().AlignCenter();
                                header.Cell().Background("#1E293B").Padding(6)
                                    .Text("Наименование").FontColor("#FFFFFF").Bold();
                                header.Cell().Background("#1E293B").Padding(6)
                                    .Text("Артикул").FontColor("#FFFFFF").Bold();
                                header.Cell().Background("#1E293B").Padding(6)
                                    .Text("Кол-во").FontColor("#FFFFFF").Bold().AlignCenter();
                                header.Cell().Background("#1E293B").Padding(6)
                                    .Text("Цена, ₽").FontColor("#FFFFFF").Bold().AlignRight();
                            });

                            decimal total = 0;
                            int i = 1;
                            foreach (var ac in assembly.AssemblyComponents)
                            {
                                var bg = i % 2 == 0 ? "#F8FAFC" : "#FFFFFF";
                                var lineTotal = ac.Component.Price * ac.Quantity;
                                total += lineTotal;

                                table.Cell().Background(bg).Padding(6)
                                    .Text(i.ToString()).AlignCenter();
                                table.Cell().Background(bg).Padding(6)
                                    .Text(ac.Component.Name);
                                table.Cell().Background(bg).Padding(6)
                                    .Text(ac.Component.SKU);
                                table.Cell().Background(bg).Padding(6)
                                    .Text(ac.Quantity.ToString()).AlignCenter();
                                table.Cell().Background(bg).Padding(6)
                                    .Text($"{lineTotal:N0}").AlignRight();
                                i++;
                            }

                            // Итог
                            table.Cell().ColumnSpan(4).Background("#E2E8F0").Padding(6)
                                .Text("Итого:").Bold().AlignRight();
                            table.Cell().Background("#E2E8F0").Padding(6)
                                .Text($"{total:N0} ₽").Bold().AlignRight();
                        });

                        col.Item().PaddingTop(6).Text(text =>
                        {
                            text.Span("Примечания: ").Bold();
                            text.Span(assembly.Notes ?? "—");
                        });

                        col.Item().PaddingVertical(20).LineHorizontal(1);

                        // Подписи
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Выдал: {issuedBy.FullName}").Bold();
                                c.Item().PaddingTop(20).Text("Подпись: _______________");
                            });
                            row.ConstantItem(40);
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Принял: {assembly.Assembler?.FullName ?? "—"}").Bold();
                                c.Item().PaddingTop(20).Text("Подпись: _______________");
                            });
                        });

                        col.Item().PaddingTop(16).AlignCenter()
                            .Text($"Дата: {DateTime.Now:dd.MM.yyyy}").FontSize(10);
                    });
                });
            }).GeneratePdf();
        }

        public byte[] GenerateReworkIssueAct(Models.Assembly assembly,
            List<AssemblyReworkItem> items, User issuedBy)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Content().Column(col =>
                    {
                        col.Item().AlignCenter().Text("ООО «Нью Ай Ти»").FontSize(14).Bold();
                        col.Item().AlignCenter().Text("АКТ ВЫДАЧИ ПО ПЕРЕДЕЛКЕ").FontSize(16).Bold();
                        col.Item().AlignCenter().Text($"Сборка {assembly.AssemblyNumber}")
                            .FontSize(12);
                        col.Item().AlignCenter().Text($"от {DateTime.Now:dd.MM.yyyy}").FontSize(11);

                        col.Item().PaddingVertical(10).LineHorizontal(1);

                        col.Item().PaddingBottom(6).Text(t =>
                        {
                            t.Span("Клиент: ").Bold();
                            t.Span(assembly.ClientName);
                        });
                        col.Item().PaddingBottom(6).Text(t =>
                        {
                            t.Span("Сборщик: ").Bold();
                            t.Span(assembly.Assembler?.FullName ?? "—");
                        });
                        col.Item().PaddingBottom(14).Text(t =>
                        {
                            t.Span("Причина переделки: ").Bold();
                            t.Span(items.FirstOrDefault()?.Notes ?? assembly.Notes ?? "—");
                        });

                        col.Item().PaddingBottom(8)
                            .Text("Перечень замены комплектующих:").Bold().FontSize(12);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.ConstantColumn(28);
                                cols.RelativeColumn(5);
                                cols.RelativeColumn(5);
                                cols.ConstantColumn(48);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Background("#1E293B").Padding(5)
                                    .Text("№").FontColor("#FFFFFF").Bold().AlignCenter();
                                h.Cell().Background("#EF4444").Padding(5)
                                    .Text("Снять (старое)").FontColor("#FFFFFF").Bold();
                                h.Cell().Background("#16A34A").Padding(5)
                                    .Text("Выдать (новое)").FontColor("#FFFFFF").Bold();
                                h.Cell().Background("#1E293B").Padding(5)
                                    .Text("Кол-во").FontColor("#FFFFFF").Bold().AlignCenter();
                            });

                            int i = 1;
                            foreach (var item in items)
                            {
                                table.Cell().Background("#FFFFFF").Padding(5)
                                    .Text(i.ToString()).AlignCenter();
                                table.Cell().Background("#FFF1F2").Padding(5)
                                    .Text(item.OldComponent?.Name ?? "— (добавить)")
                                    .FontColor("#DC2626").FontSize(10);
                                table.Cell().Background("#F0FDF4").Padding(5)
                                    .Text(item.NewComponent?.Name ?? "—")
                                    .FontColor("#15803D").Bold().FontSize(10);
                                table.Cell().Background("#FFFFFF").Padding(5)
                                    .Text(item.Quantity.ToString()).AlignCenter();
                                i++;
                            }
                        });

                        col.Item().PaddingVertical(20).LineHorizontal(1);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Выдал: {issuedBy.FullName}").Bold();
                                c.Item().PaddingTop(20).Text("Подпись: _______________");
                            });
                            row.ConstantItem(40);
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Принял: {assembly.Assembler?.FullName ?? "—"}").Bold();
                                c.Item().PaddingTop(20).Text("Подпись: _______________");
                            });
                        });

                        col.Item().PaddingTop(16).AlignCenter()
                            .Text($"Дата: {DateTime.Now:dd.MM.yyyy}").FontSize(10);
                    });
                });
            }).GeneratePdf();
        }

        // Акт тестирования
        public byte[] GenerateTestAct(Test test, Models.Assembly assembly)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Content().Column(col =>
                    {
                        col.Item().AlignCenter().Text("ООО «Нью Ай Ти»")
                            .FontSize(14).Bold();
                        col.Item().AlignCenter().Text("АКТ ТЕСТИРОВАНИЯ")
                            .FontSize(16).Bold();
                        col.Item().AlignCenter().Text($"№ {test.TestId}")
                            .FontSize(12);
                        col.Item().AlignCenter().Text($"от {test.TestDate:dd.MM.yyyy}")
                            .FontSize(11);

                        col.Item().PaddingVertical(10).LineHorizontal(1);

                        col.Item().PaddingBottom(6).Text(text =>
                        {
                            text.Span("Сборка: ").Bold();
                            text.Span(assembly.AssemblyNumber);
                        });
                        col.Item().PaddingBottom(6).Text(text =>
                        {
                            text.Span("Клиент: ").Bold();
                            text.Span(assembly.ClientName);
                        });
                        col.Item().PaddingBottom(6).Text(text =>
                        {
                            text.Span("Тестировщик: ").Bold();
                            text.Span(test.Tester?.FullName ?? "—");
                        });
                        col.Item().PaddingBottom(14).Text(text =>
                        {
                            text.Span("Дата тестирования: ").Bold();
                            text.Span(test.TestDate.ToString("dd.MM.yyyy HH:mm"));
                        });

                        // Результат
                        var resultColor = test.Result == TestResult.Passed
                            ? "#16A34A" : "#EF4444";

                        col.Item().PaddingBottom(10).Background(
                            test.Result == TestResult.Passed ? "#F0FDF4" : "#FEF2F2")
                            .Padding(12).Text(text =>
                            {
                                text.Span("Результат тестирования: ").Bold();
                                text.Span(test.Result).Bold().FontColor(resultColor);
                            });

                        if (!string.IsNullOrEmpty(test.Defects))
                        {
                            col.Item().PaddingBottom(8).Text("Выявленные дефекты:").Bold();
                            col.Item().PaddingBottom(14).Background("#FEF2F2")
                                .Padding(10).Text(test.Defects);
                        }

                        if (!string.IsNullOrEmpty(test.Notes))
                        {
                            col.Item().PaddingBottom(8).Text("Примечания:").Bold();
                            col.Item().PaddingBottom(14).Text(test.Notes);
                        }

                        // Состав сборки
                        col.Item().PaddingBottom(8).Text("Протестированная конфигурация:")
                            .Bold().FontSize(12);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);
                                columns.RelativeColumn(4);
                                columns.RelativeColumn(2);
                                columns.ConstantColumn(60);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background("#1E293B").Padding(6)
                                    .Text("№").FontColor("#FFFFFF").Bold().AlignCenter();
                                header.Cell().Background("#1E293B").Padding(6)
                                    .Text("Наименование").FontColor("#FFFFFF").Bold();
                                header.Cell().Background("#1E293B").Padding(6)
                                    .Text("Артикул").FontColor("#FFFFFF").Bold();
                                header.Cell().Background("#1E293B").Padding(6)
                                    .Text("Кол-во").FontColor("#FFFFFF").Bold().AlignCenter();
                            });

                            int i = 1;
                            foreach (var ac in assembly.AssemblyComponents)
                            {
                                var bg = i % 2 == 0 ? "#F8FAFC" : "#FFFFFF";
                                table.Cell().Background(bg).Padding(6)
                                    .Text(i.ToString()).AlignCenter();
                                table.Cell().Background(bg).Padding(6)
                                    .Text(ac.Component.Name);
                                table.Cell().Background(bg).Padding(6)
                                    .Text(ac.Component.SKU);
                                table.Cell().Background(bg).Padding(6)
                                    .Text(ac.Quantity.ToString()).AlignCenter();
                                i++;
                            }
                        });

                        col.Item().PaddingVertical(20).LineHorizontal(1);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Тестировщик: {test.Tester?.FullName ?? "—"}").Bold();
                                c.Item().PaddingTop(20).Text("Подпись: _______________");
                            });
                            row.ConstantItem(40);
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Проверил руководитель:").Bold();
                                c.Item().PaddingTop(20).Text("Подпись: _______________");
                            });
                        });

                        col.Item().PaddingTop(16).AlignCenter()
                            .Text($"Дата: {DateTime.Now:dd.MM.yyyy}").FontSize(10);
                    });
                });
            }).GeneratePdf();
        }

        // Сводный отчёт по сборкам за период
        public byte[] GenerateAssembliesReport(
            List<Models.Assembly> assemblies,
            DateTime from, DateTime to,
            int total, int completed, int inProgress,
            int rework, int lowStock, int openWarranty)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("ООО «Нью Ай Ти»").Bold().FontSize(14);
                                c.Item().Text("г. Оренбург").FontSize(10);
                            });
                            row.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().Text("СВОДНЫЙ ОТЧЁТ").Bold().FontSize(16);
                                c.Item().Text($"по производству за период").FontSize(10);
                                c.Item().Text($"{from:dd.MM.yyyy} — {to:dd.MM.yyyy}")
                                    .Bold().FontSize(11);
                            });
                        });
                        col.Item().PaddingTop(8).LineHorizontal(2);
                    });

                    page.Content().PaddingTop(16).Column(col =>
                    {
                        // KPI блок
                        col.Item().PaddingBottom(12).Text("Ключевые показатели")
                            .Bold().FontSize(13);

                        col.Item().PaddingBottom(16).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            void KpiCell(string label, string value, string color)
                            {
                                table.Cell().Border(1).BorderColor("#E2E8F0")
                                    .Background("#F8FAFC").Padding(10).Column(c =>
                                    {
                                        c.Item().Text(label).FontSize(10).FontColor("#64748B");
                                        c.Item().Text(value).Bold().FontSize(22).FontColor(color);
                                    });
                            }

                            KpiCell("Всего сборок", total.ToString(), "#1E293B");
                            KpiCell("Завершено", completed.ToString(), "#16A34A");
                            KpiCell("В работе", inProgress.ToString(), "#3B82F6");
                            KpiCell("Доработка", rework.ToString(), "#EF4444");
                            KpiCell("Мало на складе", lowStock.ToString(), "#F59E0B");
                            KpiCell("Открытые гарантии", openWarranty.ToString(), "#8B5CF6");
                        });

                        // Таблица сборок
                        col.Item().PaddingBottom(8).Text("Детализация по сборкам")
                            .Bold().FontSize(13);

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background("#1E293B").Padding(6)
                                    .Text("Номер").FontColor("#FFFFFF").Bold();
                                header.Cell().Background("#1E293B").Padding(6)
                                    .Text("Клиент").FontColor("#FFFFFF").Bold();
                                header.Cell().Background("#1E293B").Padding(6)
                                    .Text("Статус").FontColor("#FFFFFF").Bold();
                                header.Cell().Background("#1E293B").Padding(6)
                                    .Text("Сборщик").FontColor("#FFFFFF").Bold();
                                header.Cell().Background("#1E293B").Padding(6)
                                    .Text("Дата").FontColor("#FFFFFF").Bold();
                            });

                            int i = 0;
                            foreach (var a in assemblies)
                            {
                                var bg = i % 2 == 0 ? "#FFFFFF" : "#F8FAFC";
                                table.Cell().Background(bg).Padding(6)
                                    .Text(a.AssemblyNumber).FontSize(10);
                                table.Cell().Background(bg).Padding(6)
                                    .Text(a.ClientName).FontSize(10);
                                table.Cell().Background(bg).Padding(6)
                                    .Text(a.Status).FontSize(10);
                                table.Cell().Background(bg).Padding(6)
                                    .Text(a.Assembler?.FullName ?? "—").FontSize(10);
                                table.Cell().Background(bg).Padding(6)
                                    .Text(a.CreationDate.ToString("dd.MM.yyyy")).FontSize(10);
                                i++;
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Отчёт сформирован: ");
                        text.Span(DateTime.Now.ToString("dd.MM.yyyy HH:mm")).Bold();
                        text.Span(" | Маршрут сборки — ООО «Нью Ай Ти»");
                    });
                });
            }).GeneratePdf();
        }

        // Отчёт по складу
        public byte[] GenerateWarehouseReport(List<Component> components)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("ООО «Нью Ай Ти»").Bold().FontSize(14);
                                c.Item().Text("г. Оренбург").FontSize(10);
                            });
                            row.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().Text("ОТЧЁТ ПО СКЛАДУ").Bold().FontSize(16);
                                c.Item().Text($"на {DateTime.Now:dd.MM.yyyy}").FontSize(11);
                            });
                        });
                        col.Item().PaddingTop(8).LineHorizontal(2);
                    });

                    page.Content().PaddingTop(16).Column(col =>
                    {
                        col.Item().PaddingBottom(12).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.RelativeColumn(4);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.ConstantColumn(70);
                                columns.ConstantColumn(80);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background("#1E293B").Padding(6)
                                    .Text("№").FontColor("#FFFFFF").Bold().AlignCenter();
                                header.Cell().Background("#1E293B").Padding(6)
                                    .Text("Наименование").FontColor("#FFFFFF").Bold();
                                header.Cell().Background("#1E293B").Padding(6)
                                    .Text("Артикул").FontColor("#FFFFFF").Bold();
                                header.Cell().Background("#1E293B").Padding(6)
                                    .Text("Категория").FontColor("#FFFFFF").Bold();
                                header.Cell().Background("#1E293B").Padding(6)
                                    .Text("Остаток").FontColor("#FFFFFF").Bold().AlignCenter();
                                header.Cell().Background("#1E293B").Padding(6)
                                    .Text("Цена, ₽").FontColor("#FFFFFF").Bold().AlignRight();
                            });

                            int i = 1;
                            foreach (var c in components)
                            {
                                var bg = c.IsLowStock ? "#FEF2F2" :
                                         (i % 2 == 0 ? "#F8FAFC" : "#FFFFFF");
                                table.Cell().Background(bg).Padding(6)
                                    .Text(i.ToString()).AlignCenter();
                                table.Cell().Background(bg).Padding(6)
                                    .Text(c.Name);
                                table.Cell().Background(bg).Padding(6)
                                    .Text(c.SKU);
                                table.Cell().Background(bg).Padding(6)
                                    .Text(c.Category);
                                table.Cell().Background(bg).Padding(6)
                                    .Text(c.StockBalance.ToString()).AlignCenter();
                                table.Cell().Background(bg).Padding(6)
                                    .Text($"{c.Price:N0}").AlignRight();
                                i++;
                            }
                        });

                        col.Item().PaddingTop(10).Text(
                            "* Строки выделенные красным — остаток ниже минимального")
                            .FontSize(9).FontColor("#EF4444");
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Отчёт сформирован: ");
                        text.Span(DateTime.Now.ToString("dd.MM.yyyy HH:mm")).Bold();
                        text.Span(" | Маршрут сборки — ООО «Нью Ай Ти»");
                    });
                });
            }).GeneratePdf();
        }
        //техническое задание на сборку
        public byte[] GenerateAssignmentSheet(Models.Assembly assembly)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Content().Column(col =>
                    {
                        col.Item().AlignCenter().Text("ООО «Нью Ай Ти»")
                            .FontSize(14).Bold();
                        col.Item().AlignCenter().Text("ТЕХНИЧЕСКОЕ ЗАДАНИЕ НА СБОРКУ")
                            .FontSize(16).Bold();
                        col.Item().AlignCenter().Text($"№ {assembly.AssemblyNumber}")
                            .FontSize(12);
                        col.Item().AlignCenter().Text($"от {assembly.CreationDate:dd.MM.yyyy}")
                            .FontSize(11);

                        col.Item().PaddingVertical(10).LineHorizontal(1);

                        col.Item().PaddingBottom(6).Text(text =>
                        {
                            text.Span("Клиент: ").Bold();
                            text.Span(assembly.ClientName);
                        });
                        col.Item().PaddingBottom(6).Text(text =>
                        {
                            text.Span("Приоритет: ").Bold();
                            text.Span(assembly.Priority);
                        });
                        col.Item().PaddingBottom(6).Text(text =>
                        {
                            text.Span("Срок выполнения: ").Bold();
                            text.Span(assembly.Deadline?.ToString("dd.MM.yyyy") ?? "—");
                        });
                        col.Item().PaddingBottom(6).Text(text =>
                        {
                            text.Span("Диспетчер: ").Bold();
                            text.Span(assembly.Dispatcher?.FullName ?? "—");
                        });
                        col.Item().PaddingBottom(14).Text(text =>
                        {
                            text.Span("Назначен сборщик: ").Bold();
                            text.Span(assembly.Assembler?.FullName ?? "Не назначен");
                        });

                        if (!string.IsNullOrEmpty(assembly.Notes))
                        {
                            col.Item().PaddingBottom(8).Text("Примечания к конфигурации:").Bold();
                            col.Item().PaddingBottom(14).Background("#FFF7ED")
                                .Padding(10).Text(assembly.Notes);
                        }

                        col.Item().PaddingBottom(8).Text("Перечень комплектующих для сборки:")
                            .Bold().FontSize(12);

                        col.Item().PaddingBottom(16).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);
                                columns.RelativeColumn(4);
                                columns.RelativeColumn(2);
                                columns.ConstantColumn(70);
                                columns.ConstantColumn(50);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background("#1E293B").Padding(6)
                                    .Text("№").FontColor("#FFFFFF").Bold().AlignCenter();
                                header.Cell().Background("#1E293B").Padding(6)
                                    .Text("Наименование").FontColor("#FFFFFF").Bold();
                                header.Cell().Background("#1E293B").Padding(6)
                                    .Text("Артикул").FontColor("#FFFFFF").Bold();
                                header.Cell().Background("#1E293B").Padding(6)
                                    .Text("Кол-во").FontColor("#FFFFFF").Bold().AlignCenter();
                                header.Cell().Background("#1E293B").Padding(6)
                                    .Text("✓").FontColor("#FFFFFF").Bold().AlignCenter();
                            });

                            int i = 1;
                            foreach (var ac in assembly.AssemblyComponents)
                            {
                                var bg = i % 2 == 0 ? "#F8FAFC" : "#FFFFFF";
                                table.Cell().Background(bg).Padding(6)
                                    .Text(i.ToString()).AlignCenter();
                                table.Cell().Background(bg).Padding(6)
                                    .Text(ac.Component.Name);
                                table.Cell().Background(bg).Padding(6)
                                    .Text(ac.Component.SKU);
                                table.Cell().Background(bg).Padding(6)
                                    .Text(ac.Quantity.ToString()).AlignCenter();
                                table.Cell().Background(bg).Padding(6)
                                    .Text("___").AlignCenter();
                                i++;
                            }
                        });

                        col.Item().PaddingVertical(10).LineHorizontal(1);

                        col.Item().PaddingBottom(20).Text(
                            "Сборщик обязан отметить каждую позицию после установки комплектующего.")
                            .FontSize(10).FontColor("#64748B");

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Выдал диспетчер: {assembly.Dispatcher?.FullName ?? "—"}").Bold();
                                c.Item().PaddingTop(20).Text("Подпись: _______________");
                            });
                            row.ConstantItem(40);
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text($"Принял сборщик: {assembly.Assembler?.FullName ?? "—"}").Bold();
                                c.Item().PaddingTop(20).Text("Подпись: _______________");
                            });
                        });

                        col.Item().PaddingTop(16).AlignCenter()
                            .Text($"Дата: {DateTime.Now:dd.MM.yyyy}").FontSize(10);
                    });
                });
            }).GeneratePdf();
        }

        // Акт приёмки готового изделия
        public byte[] GenerateAcceptanceAct(Models.Assembly assembly, Test lastTest)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Content().Column(col =>
                    {
                        col.Item().AlignCenter().Text("ООО «Нью Ай Ти»")
                            .FontSize(14).Bold();
                        col.Item().AlignCenter().Text("АКТ ПРИЁМКИ ГОТОВОГО ИЗДЕЛИЯ")
                            .FontSize(16).Bold();
                        col.Item().AlignCenter().Text($"№ {assembly.AssemblyNumber}")
                            .FontSize(12);
                        col.Item().AlignCenter().Text($"от {DateTime.Now:dd.MM.yyyy}")
                            .FontSize(11);

                        col.Item().PaddingVertical(10).LineHorizontal(1);

                        col.Item().PaddingBottom(6).Text(text =>
                        {
                            text.Span("Заказ: ").Bold();
                            text.Span(assembly.AssemblyNumber);
                        });
                        col.Item().PaddingBottom(6).Text(text =>
                        {
                            text.Span("Клиент: ").Bold();
                            text.Span(assembly.ClientName);
                        });
                        col.Item().PaddingBottom(6).Text(text =>
                        {
                            text.Span("Сборщик: ").Bold();
                            text.Span(assembly.Assembler?.FullName ?? "—");
                        });
                        col.Item().PaddingBottom(6).Text(text =>
                        {
                            text.Span("Тестировщик: ").Bold();
                            text.Span(lastTest.Tester?.FullName ?? "—");
                        });
                        col.Item().PaddingBottom(6).Text(text =>
                        {
                            text.Span("Дата тестирования: ").Bold();
                            text.Span(lastTest.TestDate.ToString("dd.MM.yyyy"));
                        });
                        col.Item().PaddingBottom(14).Text(text =>
                        {
                            text.Span("Результат тестирования: ").Bold();
                            text.Span(lastTest.Result).Bold()
                                .FontColor(lastTest.Result == TestResult.Passed
                                    ? "#16A34A" : "#EF4444");
                        });

                        col.Item().PaddingBottom(8).Text("Состав изделия:").Bold().FontSize(12);

                        col.Item().PaddingBottom(16).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);
                                columns.RelativeColumn(4);
                                columns.RelativeColumn(2);
                                columns.ConstantColumn(70);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background("#1E293B").Padding(6)
                                    .Text("№").FontColor("#FFFFFF").Bold().AlignCenter();
                                header.Cell().Background("#1E293B").Padding(6)
                                    .Text("Наименование").FontColor("#FFFFFF").Bold();
                                header.Cell().Background("#1E293B").Padding(6)
                                    .Text("Артикул").FontColor("#FFFFFF").Bold();
                                header.Cell().Background("#1E293B").Padding(6)
                                    .Text("Кол-во").FontColor("#FFFFFF").Bold().AlignCenter();
                            });

                            int i = 1;
                            foreach (var ac in assembly.AssemblyComponents)
                            {
                                var bg = i % 2 == 0 ? "#F8FAFC" : "#FFFFFF";
                                table.Cell().Background(bg).Padding(6)
                                    .Text(i.ToString()).AlignCenter();
                                table.Cell().Background(bg).Padding(6)
                                    .Text(ac.Component.Name);
                                table.Cell().Background(bg).Padding(6)
                                    .Text(ac.Component.SKU);
                                table.Cell().Background(bg).Padding(6)
                                    .Text(ac.Quantity.ToString()).AlignCenter();
                                i++;
                            }
                        });

                        col.Item().Background("#F0FDF4").Padding(12).Text(text =>
                        {
                            text.Span("Изделие прошло контроль качества и принято к отгрузке. ")
                                .Bold();
                            text.Span($"Статус: {assembly.Status}.");
                        });

                        col.Item().PaddingVertical(20).LineHorizontal(1);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Сдал тестировщик:").Bold();
                                c.Item().Text(lastTest.Tester?.FullName ?? "—");
                                c.Item().PaddingTop(16).Text("Подпись: _______________");
                            });
                            row.ConstantItem(40);
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Принял руководитель:").Bold();
                                c.Item().PaddingTop(20).Text("Подпись: _______________");
                            });
                        });

                        col.Item().PaddingTop(16).AlignCenter()
                            .Text($"Дата: {DateTime.Now:dd.MM.yyyy}").FontSize(10);
                    });
                });
            }).GeneratePdf();
        }

        // Гарантийный талон
        public byte[] GenerateWarrantyCard(Models.Assembly assembly)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A5);
                    page.Margin(1.5f, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Content().Column(col =>
                    {
                        col.Item().Background("#1E293B").Padding(16).Column(c =>
                        {
                            c.Item().Text("ООО «Нью Ай Ти»")
                                .FontColor("#FFFFFF").FontSize(16).Bold();
                            c.Item().Text("ГАРАНТИЙНЫЙ ТАЛОН")
                                .FontColor("#94A3B8").FontSize(12);
                        });

                        col.Item().PaddingVertical(12).LineHorizontal(1);

                        col.Item().PaddingBottom(8).Text(text =>
                        {
                            text.Span("Изделие: ").Bold();
                            text.Span($"Компьютер {assembly.AssemblyNumber}");
                        });
                        col.Item().PaddingBottom(8).Text(text =>
                        {
                            text.Span("Клиент: ").Bold();
                            text.Span(assembly.ClientName);
                        });
                        col.Item().PaddingBottom(8).Text(text =>
                        {
                            text.Span("Дата продажи: ").Bold();
                            text.Span(DateTime.Now.ToString("dd.MM.yyyy"));
                        });
                        col.Item().PaddingBottom(16).Text(text =>
                        {
                            text.Span("Гарантия до: ").Bold();
                            text.Span(DateTime.Now.AddYears(1).ToString("dd.MM.yyyy"))
                                .Bold().FontColor("#16A34A");
                        });

                        col.Item().PaddingVertical(8).LineHorizontal(1);

                        col.Item().PaddingBottom(8).Text("Состав изделия:").Bold();

                        foreach (var ac in assembly.AssemblyComponents)
                        {
                            col.Item().Text($"• {ac.Component.Name} — {ac.Quantity} шт.")
                                .FontSize(10);
                        }

                        col.Item().PaddingVertical(12).LineHorizontal(1);

                        col.Item().PaddingBottom(6).Text("Условия гарантии:")
                            .Bold().FontSize(10);
                        col.Item().Text("— Гарантийный срок 12 месяцев с даты продажи")
                            .FontSize(9).FontColor("#64748B");
                        col.Item().Text("— Гарантия не распространяется на механические повреждения")
                            .FontSize(9).FontColor("#64748B");
                        col.Item().Text("— При обращении предъявите данный талон")
                            .FontSize(9).FontColor("#64748B");

                        col.Item().PaddingVertical(12).LineHorizontal(1);

                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Продавец:").Bold().FontSize(10);
                                c.Item().PaddingTop(16).Text("Подпись: ___________").FontSize(10);
                            });
                            row.ConstantItem(20);
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Покупатель:").Bold().FontSize(10);
                                c.Item().PaddingTop(16).Text("Подпись: ___________").FontSize(10);
                            });
                        });
                    });
                });
            }).GeneratePdf();
        }
        public byte[] GenerateAssemblyLabel(Models.Assembly assembly)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A6);
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    page.Content().Column(col =>
                    {
                        // Шапка
                        col.Item().Background("#1E293B").Padding(10).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("ООО «Нью Ай Ти»")
                                    .FontColor("#FFFFFF").Bold().FontSize(12);
                                c.Item().Text("NITRINOnet")
                                    .FontColor("#94A3B8").FontSize(9);
                            });
                        });

                        col.Item().PaddingVertical(8).LineHorizontal(1);

                        // Серийный номер крупно
                        col.Item().AlignCenter().Text("СЕРИЙНЫЙ НОМЕР")
                            .FontSize(9).FontColor("#64748B");
                        col.Item().AlignCenter().Text(assembly.AssemblyNumber)
                            .FontSize(22).Bold().FontColor("#1E293B");

                        col.Item().PaddingVertical(6).LineHorizontal(1);

                        // Данные
                        col.Item().PaddingBottom(4).Text(text =>
                        {
                            text.Span("Клиент: ").Bold().FontSize(9);
                            text.Span(assembly.ClientName).FontSize(9);
                        });
                        col.Item().PaddingBottom(4).Text(text =>
                        {
                            text.Span("Сборщик: ").Bold().FontSize(9);
                            text.Span(assembly.Assembler?.FullName ?? "—").FontSize(9);
                        });
                        col.Item().PaddingBottom(4).Text(text =>
                        {
                            text.Span("Дата сборки: ").Bold().FontSize(9);
                            text.Span(assembly.CreationDate.ToString("dd.MM.yyyy")).FontSize(9);
                        });
                        col.Item().PaddingBottom(4).Text(text =>
                        {
                            text.Span("Статус: ").Bold().FontSize(9);
                            text.Span(assembly.Status).FontSize(9);
                        });

                        col.Item().PaddingVertical(6).LineHorizontal(1);

                        // Состав кратко
                        col.Item().PaddingBottom(4).Text("Конфигурация:").Bold().FontSize(9);
                        foreach (var ac in assembly.AssemblyComponents.Take(6))
                        {
                            col.Item().Text($"• {ac.Component.Name} x{ac.Quantity}")
                                .FontSize(8).FontColor("#475569");
                        }
                        if (assembly.AssemblyComponents.Count > 6)
                        {
                            col.Item().Text($"• и ещё {assembly.AssemblyComponents.Count - 6} позиций...")
                                .FontSize(8).FontColor("#94A3B8");
                        }

                        col.Item().PaddingVertical(6).LineHorizontal(1);

                        // QR-код
                        col.Item().AlignCenter().PaddingTop(4).Column(c =>
                        {
                            try
                            {
                                var qrGenerator = new QRCoder.QRCodeGenerator();
                                var qrData = qrGenerator.CreateQrCode(
                                    $"NITRINO|{assembly.AssemblyNumber}|{assembly.ClientName}|{assembly.CreationDate:dd.MM.yyyy}",
                                    QRCoder.QRCodeGenerator.ECCLevel.Q);
                                var qrCode = new QRCoder.PngByteQRCode(qrData);
                                var qrBytes = qrCode.GetGraphic(4);

                                c.Item().AlignCenter().Image(qrBytes).FitWidth();
                                c.Item().AlignCenter().Text("Сканируйте для просмотра информации")
                                    .FontSize(7).FontColor("#94A3B8");
                            }
                            catch
                            {
                                c.Item().AlignCenter().Text(assembly.AssemblyNumber)
                                    .FontSize(8).FontColor("#94A3B8");
                            }
                        });
                    });
                });
            }).GeneratePdf();
        }

        // Статистика по сборщикам
        public byte[] GenerateAssemblerStatsPdf(
            List<ViewModels.AssemblerStat> stats, DateTime dateFrom, DateTime dateTo)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("ООО «Нью Ай Ти»").Bold().FontSize(14);
                                c.Item().Text("г. Оренбург").FontSize(10);
                            });
                            row.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().Text("СТАТИСТИКА ПО СБОРЩИКАМ").Bold().FontSize(14);
                                c.Item().Text($"Период: {dateFrom:dd.MM.yyyy} — {dateTo:dd.MM.yyyy}").FontSize(11);
                            });
                        });
                        col.Item().PaddingTop(8).LineHorizontal(2);
                    });

                    page.Content().PaddingTop(16).Column(col =>
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                            });

                            table.Header(h =>
                            {
                                h.Cell().Background("#1E293B").Padding(8)
                                    .Text("№").FontColor("#FFFFFF").Bold();
                                h.Cell().Background("#1E293B").Padding(8)
                                    .Text("Сборщик").FontColor("#FFFFFF").Bold();
                                h.Cell().Background("#1E293B").Padding(8)
                                    .Text("Всего").FontColor("#FFFFFF").Bold().AlignCenter();
                                h.Cell().Background("#1E293B").Padding(8)
                                    .Text("Готово").FontColor("#FFFFFF").Bold().AlignCenter();
                                h.Cell().Background("#1E293B").Padding(8)
                                    .Text("Доработка").FontColor("#FFFFFF").Bold().AlignCenter();
                                h.Cell().Background("#1E293B").Padding(8)
                                    .Text("% выполн.").FontColor("#FFFFFF").Bold().AlignCenter();
                            });

                            var i = 1;
                            foreach (var s in stats)
                            {
                                var bg = i % 2 == 0 ? "#F8FAFC" : "#FFFFFF";
                                table.Cell().Background(bg).Padding(8)
                                    .Text(i.ToString()).FontSize(10);
                                table.Cell().Background(bg).Padding(8)
                                    .Text(s.Name).FontSize(10);
                                table.Cell().Background(bg).Padding(8)
                                    .Text(s.Total.ToString()).FontSize(10).AlignCenter();
                                table.Cell().Background(bg).Padding(8)
                                    .Text(s.Completed.ToString()).FontSize(10).FontColor("#16A34A").AlignCenter();
                                table.Cell().Background(bg).Padding(8)
                                    .Text(s.Rework.ToString()).FontSize(10).FontColor("#EF4444").AlignCenter();
                                table.Cell().Background(bg).Padding(8)
                                    .Text(s.CompletionRate).FontSize(10).FontColor("#8B5CF6").AlignCenter();
                                i++;
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text(text =>
                    {
                        text.Span("Отчёт сформирован: ");
                        text.Span(DateTime.Now.ToString("dd.MM.yyyy HH:mm")).Bold();
                        text.Span(" | Маршрут сборки — ООО «Нью Ай Ти»");
                    });
                });
            }).GeneratePdf();
        }

        // Сохранить PDF и открыть
        public void SaveAndOpen(byte[] pdf, string fileName)
        {
            var dir = Helpers.AppSettings.OutputDirectory;
            var path = Path.Combine(dir, fileName);
            File.WriteAllBytes(path, pdf);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
        }
    }
}