using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using МаршрутСборки.Data;
using МаршрутСборки.Helpers;
using МаршрутСборки.Models;
using МаршрутСборки.Services;

namespace МаршрутСборки.ViewModels
{
    public class AssemblerStat
    {
        public string Name { get; set; } = string.Empty;
        public int Total { get; set; }
        public int Completed { get; set; }
        public int InProgress { get; set; }
        public int Rework { get; set; }
        public string CompletionRate => Total > 0 ? $"{Completed * 100 / Total}%" : "—";
    }

    public class ChartBar
    {
        public int Value { get; set; }
        public double BarHeight { get; set; }
    }

    public class AssemblerChartGroup
    {
        public string Name { get; set; } = string.Empty;
        public ChartBar Total { get; set; } = new();
        public ChartBar Completed { get; set; } = new();
        public ChartBar Rework { get; set; } = new();
    }

    public class YAxisLabel
    {
        public string Text { get; set; } = string.Empty;
        public double TopOffset { get; set; }
    }

    public class ReportsViewModel : BaseViewModel
    {
        private int _totalAssemblies;
        private int _completedAssemblies;
        private int _inProgressAssemblies;
        private int _reworkAssemblies;
        private int _shippedAssemblies;
        private int _lowStockCount;
        private int _openWarrantyCases;
        private DateTime _dateFrom = new DateTime(2026, 4, 1);
        private DateTime _dateTo = new DateTime(2026, 6, 30);
        private User? _selectedAssemblerFilter;

        public int TotalAssemblies
        {
            get => _totalAssemblies;
            set => SetProperty(ref _totalAssemblies, value);
        }
        public int CompletedAssemblies
        {
            get => _completedAssemblies;
            set => SetProperty(ref _completedAssemblies, value);
        }
        public int InProgressAssemblies
        {
            get => _inProgressAssemblies;
            set => SetProperty(ref _inProgressAssemblies, value);
        }
        public int ReworkAssemblies
        {
            get => _reworkAssemblies;
            set => SetProperty(ref _reworkAssemblies, value);
        }
        public int ShippedAssemblies
        {
            get => _shippedAssemblies;
            set => SetProperty(ref _shippedAssemblies, value);
        }
        public int LowStockCount
        {
            get => _lowStockCount;
            set => SetProperty(ref _lowStockCount, value);
        }
        public int OpenWarrantyCases
        {
            get => _openWarrantyCases;
            set => SetProperty(ref _openWarrantyCases, value);
        }

        public DateTime DateFrom
        {
            get => _dateFrom;
            set { if (SetProperty(ref _dateFrom, value)) Load(); }
        }

        public DateTime DateTo
        {
            get => _dateTo;
            set { if (SetProperty(ref _dateTo, value)) Load(); }
        }

        public User? SelectedAssemblerFilter
        {
            get => _selectedAssemblerFilter;
            set { if (SetProperty(ref _selectedAssemblerFilter, value)) Load(); }
        }

        public List<User?> Assemblers { get; private set; } = new();

        public ObservableCollection<EventLog> RecentEvents { get; } = new();
        public ObservableCollection<AssemblerStat> AssemblerStats { get; } = new();
        public ObservableCollection<WarehouseOperation> WarehouseOps { get; } = new();
        public ObservableCollection<Models.Assembly> ShippedList { get; } = new();

        public List<AssemblerChartGroup> ChartGroups { get; private set; } = new();
        public List<YAxisLabel> YAxisLabels { get; private set; } = new();
        public bool HasChartData => ChartGroups.Count > 0;
        public bool HasNoChartData => ChartGroups.Count == 0;

        public ICommand RefreshCommand { get; }
        public ICommand ExportAssembliesReportCommand { get; }
        public ICommand ExportWarehouseReportCommand { get; }
        public ICommand ExportEventLogTxtCommand { get; }
        public ICommand ExportWarehouseOpsTxtCommand { get; }
        public ICommand ExportAssemblerStatsPdfCommand { get; }
        public ICommand ExportShippedTxtCommand { get; }

        public ReportsViewModel()
        {
            RefreshCommand = new RelayCommand(_ => Load());
            ExportAssembliesReportCommand = new RelayCommand(_ => ExportAssembliesReport());
            ExportWarehouseReportCommand = new RelayCommand(_ => ExportWarehouseReport());
            ExportEventLogTxtCommand = new RelayCommand(_ => ExportEventLogTxt());
            ExportWarehouseOpsTxtCommand = new RelayCommand(_ => ExportWarehouseOpsTxt());
            ExportAssemblerStatsPdfCommand = new RelayCommand(_ => ExportAssemblerStatsPdf());
            ExportShippedTxtCommand = new RelayCommand(_ => ExportShippedTxt());
            Load();
        }

        private void Load()
        {
            var context = new AppDbContext();
            var dateFromUtc = DateFrom.ToUniversalTime();
            var dateToUtc = DateTo.AddDays(1).ToUniversalTime();

            // Populate assembler filter list once
            if (Assemblers.Count == 0)
            {
                var assemblers = context.Users
                    .Include(u => u.Role)
                    .Where(u => u.Role.RoleName == "Сборщик" && u.IsActive)
                    .OrderBy(u => u.LastName)
                    .ToList();
                Assemblers = new List<User?> { null }.Concat(assemblers.Cast<User?>()).ToList();
                OnPropertyChanged(nameof(Assemblers));
            }

            var assembliesQuery = context.Assemblies
                .Include(a => a.Assembler)
                .Where(a => a.CreationDate >= dateFromUtc && a.CreationDate <= dateToUtc);

            if (SelectedAssemblerFilter != null)
                assembliesQuery = assembliesQuery.Where(a => a.AssemblerId == SelectedAssemblerFilter.UserId);

            var assemblies = assembliesQuery.ToList();

            TotalAssemblies = assemblies.Count;
            CompletedAssemblies = assemblies.Count(a =>
                a.Status == AssemblyStatus.Ready || a.Status == AssemblyStatus.Shipped);
            InProgressAssemblies = assemblies.Count(a => a.Status == AssemblyStatus.InProgress);
            ReworkAssemblies = assemblies.Count(a => a.Status == AssemblyStatus.Rework);
            LowStockCount = context.Components.Count(c => c.StockBalance <= c.MinStock);
            OpenWarrantyCases = context.WarrantyCases.Count(w => w.Status != WarrantyStatus.Closed);

            // Shipped in period — by ShippedDate if set, otherwise show all Shipped created in period
            var shipped = context.Assemblies
                .Include(a => a.Assembler)
                .Include(a => a.Dispatcher)
                .Where(a => a.Status == AssemblyStatus.Shipped &&
                            ((a.ShippedDate != null && a.ShippedDate >= dateFromUtc && a.ShippedDate <= dateToUtc) ||
                             (a.ShippedDate == null && a.CreationDate >= dateFromUtc && a.CreationDate <= dateToUtc)))
                .OrderByDescending(a => a.ShippedDate ?? a.CreationDate)
                .ToList();
            if (SelectedAssemblerFilter != null)
                shipped = shipped.Where(a => a.AssemblerId == SelectedAssemblerFilter.UserId).ToList();
            ShippedAssemblies = shipped.Count;
            ShippedList.Clear();
            foreach (var a in shipped)
                ShippedList.Add(a);

            // Assembler stats — always all assemblers in the period
            var allAssemblies = context.Assemblies
                .Include(a => a.Assembler)
                .Where(a => a.CreationDate >= dateFromUtc && a.CreationDate <= dateToUtc &&
                            a.AssemblerId != null)
                .ToList();

            var statGroups = allAssemblies
                .GroupBy(a => a.Assembler!)
                .Select(g => new AssemblerStat
                {
                    Name = g.Key.FullName,
                    Total = g.Count(),
                    Completed = g.Count(a => a.Status == AssemblyStatus.Ready || a.Status == AssemblyStatus.Shipped),
                    InProgress = g.Count(a => a.Status == AssemblyStatus.InProgress),
                    Rework = g.Count(a => a.Status == AssemblyStatus.Rework)
                })
                .OrderByDescending(s => s.Total)
                .ToList();

            AssemblerStats.Clear();
            foreach (var s in statGroups)
                AssemblerStats.Add(s);

            // Build WPF bar chart data
            const double barAreaHeight = 110;
            int maxVal = statGroups.Count > 0 ? Math.Max(1, statGroups.Max(s => s.Total)) : 1;
            double Scale(int v) => v == 0 ? 0 : Math.Max(2, v * barAreaHeight / maxVal);

            ChartGroups = statGroups.Select(s => new AssemblerChartGroup
            {
                Name      = s.Name,
                Total     = new ChartBar { Value = s.Total,     BarHeight = Scale(s.Total) },
                Completed = new ChartBar { Value = s.Completed, BarHeight = Scale(s.Completed) },
                Rework    = new ChartBar { Value = s.Rework,    BarHeight = Scale(s.Rework) }
            }).ToList();

            int steps = Math.Min(maxVal, 5);
            YAxisLabels = Enumerable.Range(0, steps + 1)
                .Select(i =>
                {
                    int v = (int)Math.Round((double)maxVal * i / steps);
                    double top = barAreaHeight * (1.0 - (double)i / steps) - 6;
                    return new YAxisLabel { Text = v.ToString(), TopOffset = Math.Max(0, top) };
                })
                .ToList();

            OnPropertyChanged(nameof(ChartGroups));
            OnPropertyChanged(nameof(YAxisLabels));
            OnPropertyChanged(nameof(HasChartData));
            OnPropertyChanged(nameof(HasNoChartData));

            // Event log
            var eventsQuery = context.EventLogs
                .Include(e => e.User)
                .Where(e => e.ActionTime >= dateFromUtc && e.ActionTime <= dateToUtc);

            if (SelectedAssemblerFilter != null)
                eventsQuery = eventsQuery.Where(e => e.UserId == SelectedAssemblerFilter.UserId);

            var events = eventsQuery
                .OrderByDescending(e => e.ActionTime)
                .Take(100)
                .ToList();

            RecentEvents.Clear();
            foreach (var e in events)
                RecentEvents.Add(e);

            // Warehouse operations
            var warehouseSvc = new WarehouseService(context);
            var ops = warehouseSvc.GetHistoryFiltered(dateFromUtc, dateToUtc);
            if (SelectedAssemblerFilter != null)
                ops = ops.Where(o => o.UserId == SelectedAssemblerFilter.UserId).ToList();

            WarehouseOps.Clear();
            foreach (var op in ops)
                WarehouseOps.Add(op);
        }

        private void ExportEventLogTxt()
        {
            var dlg = new SaveFileDialog
            {
                Title = "Сохранить журнал событий",
                Filter = "Текстовый файл (*.txt)|*.txt",
                FileName = $"Журнал_событий_{DateFrom:dd.MM.yyyy}_{DateTo:dd.MM.yyyy}.txt"
            };
            if (dlg.ShowDialog() != true) return;

            var sb = new StringBuilder();
            sb.AppendLine("ЖУРНАЛ СОБЫТИЙ");
            sb.AppendLine($"Период: {DateFrom:dd.MM.yyyy} — {DateTo:dd.MM.yyyy}");
            if (SelectedAssemblerFilter != null)
                sb.AppendLine($"Сотрудник: {SelectedAssemblerFilter.FullName}");
            sb.AppendLine($"Дата выгрузки: {DateTime.Now:dd.MM.yyyy HH:mm}");
            sb.AppendLine(new string('=', 80));
            sb.AppendLine();

            foreach (var ev in RecentEvents)
            {
                sb.AppendLine($"[{ev.ActionTime.ToLocalTime():dd.MM.yyyy HH:mm:ss}]  {ev.User?.FullName ?? "—"}");
                sb.AppendLine($"  Действие:  {ev.ActionType}");
                sb.AppendLine($"  Описание:  {ev.Description}");
                sb.AppendLine(new string('-', 60));
            }

            File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
            MessageBox.Show($"Журнал сохранён:\n{dlg.FileName}", "Экспорт завершён",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportAssembliesReport()
        {
            var context = new AppDbContext();
            var dateFromUtc = DateFrom.ToUniversalTime();
            var dateToUtc = DateTo.AddDays(1).ToUniversalTime();

            var query = context.Assemblies
                .Include(a => a.Assembler)
                .Include(a => a.Dispatcher)
                .Include(a => a.AssemblyComponents).ThenInclude(ac => ac.Component)
                .Where(a => a.CreationDate >= dateFromUtc && a.CreationDate <= dateToUtc);

            if (SelectedAssemblerFilter != null)
                query = query.Where(a => a.AssemblerId == SelectedAssemblerFilter.UserId);

            var assemblies = query.OrderByDescending(a => a.CreationDate).ToList();

            var pdfService = new PdfService();
            var pdf = pdfService.GenerateAssembliesReport(
                assemblies, DateFrom, DateTo,
                TotalAssemblies, CompletedAssemblies,
                InProgressAssemblies, ReworkAssemblies,
                LowStockCount, OpenWarrantyCases);

            pdfService.SaveAndOpen(pdf,
                $"Отчёт_по_сборкам_{DateFrom:dd.MM.yyyy}_{DateTo:dd.MM.yyyy}.pdf");
        }

        private void ExportWarehouseReport()
        {
            var context = new AppDbContext();
            var components = context.Components
                .OrderBy(c => c.Category).ThenBy(c => c.Name)
                .ToList();

            var pdfService = new PdfService();
            var pdf = pdfService.GenerateWarehouseReport(components);
            pdfService.SaveAndOpen(pdf,
                $"Отчёт_по_складу_{DateTime.Now:dd.MM.yyyy}.pdf");
        }

        private void ExportWarehouseOpsTxt()
        {
            var dlg = new SaveFileDialog
            {
                Title = "Сохранить складские операции",
                Filter = "Текстовый файл (*.txt)|*.txt",
                FileName = $"Склад_{DateFrom:dd.MM.yyyy}_{DateTo:dd.MM.yyyy}.txt"
            };
            if (dlg.ShowDialog() != true) return;

            var sb = new StringBuilder();
            sb.AppendLine("СКЛАДСКИЕ ОПЕРАЦИИ");
            sb.AppendLine($"Период: {DateFrom:dd.MM.yyyy} — {DateTo:dd.MM.yyyy}");
            if (SelectedAssemblerFilter != null)
                sb.AppendLine($"Сотрудник: {SelectedAssemblerFilter.FullName}");
            sb.AppendLine($"Дата выгрузки: {DateTime.Now:dd.MM.yyyy HH:mm}");
            sb.AppendLine(new string('=', 80));
            sb.AppendLine();
            sb.AppendLine($"{"Дата",-20} {"Тип",-14} {"Комплектующее",-30} {"Кол.",-6} {"Сборка",-16} {"Кладовщик",-20}");
            sb.AppendLine(new string('-', 80));

            foreach (var op in WarehouseOps)
            {
                sb.AppendLine(
                    $"{op.OperationDate.ToLocalTime():dd.MM.yyyy HH:mm,-20} " +
                    $"{op.OperationType,-14} " +
                    $"{(op.Component?.Name ?? "—"),-30} " +
                    $"{op.Quantity,-6} " +
                    $"{(op.Assembly?.AssemblyNumber ?? "—"),-16} " +
                    $"{(op.User?.FullName ?? "—"),-20}");
            }

            File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
            MessageBox.Show($"Файл сохранён:\n{dlg.FileName}", "Экспорт завершён",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExportAssemblerStatsPdf()
        {
            var pdfService = new PdfService();
            var stats = AssemblerStats.ToList();
            if (stats.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта.",
                    "Нет данных", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var pdf = pdfService.GenerateAssemblerStatsPdf(stats, DateFrom, DateTo);
            pdfService.SaveAndOpen(pdf,
                $"Статистика_сборщиков_{DateFrom:dd.MM.yyyy}_{DateTo:dd.MM.yyyy}.pdf");
        }

        private void ExportShippedTxt()
        {
            if (ShippedList.Count == 0)
            {
                MessageBox.Show("Нет отгруженных сборок за выбранный период.",
                    "Нет данных", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dlg = new SaveFileDialog
            {
                Title = "Сохранить историю отгрузок",
                Filter = "Текстовый файл (*.txt)|*.txt",
                FileName = $"Отгрузки_{DateFrom:dd.MM.yyyy}_{DateTo:dd.MM.yyyy}.txt"
            };
            if (dlg.ShowDialog() != true) return;

            var sb = new StringBuilder();
            sb.AppendLine("ИСТОРИЯ ОТГРУЗОК");
            sb.AppendLine($"Период: {DateFrom:dd.MM.yyyy} — {DateTo:dd.MM.yyyy}");
            if (SelectedAssemblerFilter != null)
                sb.AppendLine($"Сборщик: {SelectedAssemblerFilter.FullName}");
            sb.AppendLine($"Дата выгрузки: {DateTime.Now:dd.MM.yyyy HH:mm}");
            sb.AppendLine($"Всего отгружено: {ShippedList.Count} шт.");
            sb.AppendLine(new string('=', 80));
            sb.AppendLine();
            sb.AppendLine($"{"Дата отгрузки",-22} {"Номер",-16} {"Клиент",-28} {"Сборщик",-22} {"Диспетчер",-20}");
            sb.AppendLine(new string('-', 80));

            foreach (var a in ShippedList)
            {
                var date = a.ShippedDate.HasValue
                    ? a.ShippedDate.Value.ToLocalTime().ToString("dd.MM.yyyy HH:mm")
                    : "—";
                sb.AppendLine(
                    $"{date,-22} " +
                    $"{a.AssemblyNumber,-16} " +
                    $"{a.ClientName,-28} " +
                    $"{(a.Assembler?.FullName ?? "—"),-22} " +
                    $"{(a.Dispatcher?.FullName ?? "—"),-20}");
            }

            File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
            MessageBox.Show($"Файл сохранён:\n{dlg.FileName}", "Экспорт завершён",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
