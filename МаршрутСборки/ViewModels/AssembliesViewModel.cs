using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using МаршрутСборки.Data;
using МаршрутСборки.Helpers;
using МаршрутСборки.Models;
using МаршрутСборки.Services;

namespace МаршрутСборки.ViewModels
{
    public class AssembliesViewModel : BaseViewModel
    {
        private AssemblyService _assemblyService;

        private ObservableCollection<Assembly> _assemblies = new();
        private Assembly? _selectedAssembly;
        private List<Assembly> _selectedAssemblies = new();

        public List<Assembly> SelectedAssemblies
        {
            get => _selectedAssemblies;
            set
            {
                _selectedAssemblies = value;
                OnPropertyChanged(nameof(SelectedAssemblies));
                OnPropertyChanged(nameof(CanShipSelected));
                OnPropertyChanged(nameof(ShipSelectedLabel));
            }
        }

        public bool CanShipSelected =>
            (SessionContext.IsDispatcher || SessionContext.IsAdmin) &&
            _selectedAssemblies.Any(a => a.Status == AssemblyStatus.Ready);

        public bool CanShipAllReady =>
            SessionContext.IsDispatcher || SessionContext.IsAdmin;

        public string ShipSelectedLabel =>
            _selectedAssemblies.Count(a => a.Status == AssemblyStatus.Ready) is int n && n > 0
                ? $"Отправить выбранные ({n})"
                : "Отправить выбранные";

        private ObservableCollection<AssemblyReworkItem> _reworkItems = new();
        private ObservableCollection<AssemblyReworkItem> _completedReworkItems = new();
        private WarrantyCase? _linkedWarrantyCase;
        private ObservableCollection<WarrantyCaseNote> _warrantyNotes = new();
        private string _searchText = string.Empty;
        private string _selectedStatusFilter =
            SessionContext.IsAssembler ? "Мои" : "Все";

        public string PageTitle => "Сборки";
        public string PageSubtitle =>
            SessionContext.IsAssembler
                ? "Ваши активные сборки и доступные заказы"
                : "Управление заказами на сборку";

        public ObservableCollection<Assembly> Assemblies
        {
            get => _assemblies;
            set => SetProperty(ref _assemblies, value);
        }

        public ObservableCollection<AssemblyReworkItem> ReworkItems
        {
            get => _reworkItems;
            set => SetProperty(ref _reworkItems, value);
        }

        public ObservableCollection<AssemblyReworkItem> CompletedReworkItems
        {
            get => _completedReworkItems;
            set => SetProperty(ref _completedReworkItems, value);
        }

        public bool HasReworkItems => _reworkItems.Count > 0;
        public bool HasCompletedReworkItems => _completedReworkItems.Count > 0;

        public WarrantyCase? LinkedWarrantyCase
        {
            get => _linkedWarrantyCase;
            set
            {
                SetProperty(ref _linkedWarrantyCase, value);
                OnPropertyChanged(nameof(HasWarrantyInfo));
            }
        }

        public ObservableCollection<WarrantyCaseNote> WarrantyNotes
        {
            get => _warrantyNotes;
            set => SetProperty(ref _warrantyNotes, value);
        }

        public bool HasWarrantyInfo => _linkedWarrantyCase != null;

        public Assembly? SelectedAssembly
        {
            get => _selectedAssembly;
            set
            {
                if (SetProperty(ref _selectedAssembly, value))
                {
                    OnPropertyChanged(nameof(CanAccept));
                    OnPropertyChanged(nameof(CanComplete));
                    OnPropertyChanged(nameof(CanSendToTest));
                    OnPropertyChanged(nameof(CanEdit));
                    OnPropertyChanged(nameof(CanDelete));
                    OnPropertyChanged(nameof(CanRework));
                    OnPropertyChanged(nameof(CanPrintLabel));
                    OnPropertyChanged(nameof(ShowWaitingForComponents));
                    LoadReworkItems();
                    LoadWarrantyInfo();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    LoadAssemblies();
            }
        }

        public string SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                if (SetProperty(ref _selectedStatusFilter, value))
                    LoadAssemblies();
            }
        }

        public List<string> StatusFilters =>
            SessionContext.IsAssembler
                ? new List<string>
                {
                    "Мои",
                    "Все",
                    AssemblyStatus.New,
                    AssemblyStatus.InProgress,
                    AssemblyStatus.OnTesting,
                    AssemblyStatus.Ready,
                    AssemblyStatus.Rework
                }
                : new List<string>
                {
                    "Все",
                    AssemblyStatus.New,
                    AssemblyStatus.InProgress,
                    AssemblyStatus.ReadyForTest,
                    AssemblyStatus.OnTesting,
                    AssemblyStatus.Ready,
                    AssemblyStatus.Rework,
                    AssemblyStatus.Shipped
                };

        public bool CanCreate =>
            SessionContext.IsAdmin || SessionContext.IsDispatcher;

        public bool CanAccept =>
            SessionContext.IsAssembler &&
            SelectedAssembly?.Status == AssemblyStatus.New;

        public bool CanComplete =>
            SessionContext.IsAssembler &&
            SelectedAssembly?.Status == AssemblyStatus.InProgress &&
            HasComponentsIssued(SelectedAssembly.AssemblyId) &&
            !HasPendingReworkItems(SelectedAssembly.AssemblyId);

        public bool CanSendToTest =>
            (SessionContext.IsAssembler || SessionContext.IsAdmin) &&
            SelectedAssembly?.Status == AssemblyStatus.ReadyForTest;

        public bool CanRework =>
            SessionContext.IsAssembler &&
            SelectedAssembly?.Status == AssemblyStatus.Rework;

        public bool CanPrintLabel =>
            SelectedAssembly?.Status == AssemblyStatus.Ready ||
            SelectedAssembly?.Status == AssemblyStatus.Shipped;

        private bool HasComponentsIssued(int assemblyId)
        {
            var context = new AppDbContext();
            return context.WarehouseOperations.Any(
                o => o.AssemblyId == assemblyId &&
                     o.OperationType == OperationType.Issue);
        }

        private bool HasPendingReworkItems(int assemblyId)
        {
            var context = new AppDbContext();
            return context.AssemblyReworkItems.Any(
                r => r.AssemblyId == assemblyId && !r.IsIssued);
        }

        public bool ShowWaitingForComponents =>
            SessionContext.IsAssembler &&
            SelectedAssembly?.Status == AssemblyStatus.InProgress &&
            SelectedAssembly != null &&
            (!HasComponentsIssued(SelectedAssembly.AssemblyId) ||
             HasPendingReworkItems(SelectedAssembly.AssemblyId));

        public bool CanEdit =>
            (SessionContext.IsAdmin || SessionContext.IsDispatcher) &&
            SelectedAssembly?.Status == AssemblyStatus.New;

        public bool CanDelete =>
            SessionContext.IsAdmin && SelectedAssembly != null;

        public ICommand CreateCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand AcceptCommand { get; }
        public ICommand CompleteCommand { get; }
        public ICommand SendToTestCommand { get; }
        public ICommand ReworkCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand PrintAssignmentCommand { get; }
        public ICommand PrintWarrantyCommand { get; }
        public ICommand PrintAcceptanceCommand { get; }
        public ICommand PrintLabelCommand { get; }
        public ICommand ShipSelectedCommand { get; }
        public ICommand ShipAllReadyCommand { get; }

        public AssembliesViewModel()
        {
            var context = new AppDbContext();
            _assemblyService = new AssemblyService(context);

            CreateCommand = new RelayCommand(
                _ => CreateAssembly(),
                _ => CanCreate);

            EditCommand = new RelayCommand(
                _ => EditAssembly(),
                _ => CanEdit);

            AcceptCommand = new RelayCommand(
                _ => AcceptAssembly(),
                _ => CanAccept);

            CompleteCommand = new RelayCommand(
                _ => CompleteAssembly(),
                _ => CanComplete);

            SendToTestCommand = new RelayCommand(
                _ => SendToTest(),
                _ => CanSendToTest);

            ReworkCommand = new RelayCommand(
                _ => TakeRework(),
                _ => CanRework);

            DeleteCommand = new RelayCommand(
                _ => DeleteAssembly(),
                _ => CanDelete);

            PrintAssignmentCommand = new RelayCommand(
                _ => PrintAssignment(),
                _ => SelectedAssembly != null);

            PrintWarrantyCommand = new RelayCommand(
                _ => PrintWarranty(),
                _ => SelectedAssembly?.Status == AssemblyStatus.Ready ||
                     SelectedAssembly?.Status == AssemblyStatus.Shipped);

            PrintAcceptanceCommand = new RelayCommand(
                _ => PrintAcceptance(),
                _ => SelectedAssembly?.Status == AssemblyStatus.Ready ||
                     SelectedAssembly?.Status == AssemblyStatus.Shipped);

            PrintLabelCommand = new RelayCommand(
                _ => PrintLabel(),
                _ => CanPrintLabel);

            ShipSelectedCommand = new RelayCommand(
                _ => ShipSelected(),
                _ => CanShipSelected);

            ShipAllReadyCommand = new RelayCommand(
                _ => ShipAllReady(),
                _ => CanShipAllReady);

            LoadAssemblies();
        }

        private void LoadReworkItems()
        {
            if (SelectedAssembly == null)
            {
                ReworkItems = new();
                CompletedReworkItems = new();
                OnPropertyChanged(nameof(HasReworkItems));
                OnPropertyChanged(nameof(HasCompletedReworkItems));
                return;
            }

            var context = new AppDbContext();
            var all = context.AssemblyReworkItems
                .Include(r => r.OldComponent)
                .Include(r => r.NewComponent)
                .Where(r => r.AssemblyId == SelectedAssembly.AssemblyId)
                .ToList();

            ReworkItems = new ObservableCollection<AssemblyReworkItem>(
                all.Where(r => !r.IsIssued));
            CompletedReworkItems = new ObservableCollection<AssemblyReworkItem>(
                all.Where(r => r.IsIssued));

            OnPropertyChanged(nameof(HasReworkItems));
            OnPropertyChanged(nameof(HasCompletedReworkItems));
        }

        private void LoadWarrantyInfo()
        {
            if (SelectedAssembly == null)
            {
                LinkedWarrantyCase = null;
                WarrantyNotes = new();
                return;
            }

            var context = new AppDbContext();
            var warrantyCase = context.WarrantyCases
                .Include(c => c.Engineer)
                .Where(c => c.AssemblyId == SelectedAssembly.AssemblyId &&
                            c.Status != WarrantyStatus.Closed)
                .OrderByDescending(c => c.ReceivedDate)
                .FirstOrDefault();

            LinkedWarrantyCase = warrantyCase;

            if (warrantyCase != null)
            {
                var notes = context.WarrantyCaseNotes
                    .Include(n => n.Author)
                    .Where(n => n.CaseId == warrantyCase.CaseId)
                    .OrderBy(n => n.CreatedAt)
                    .ToList();
                WarrantyNotes = new ObservableCollection<WarrantyCaseNote>(notes);
            }
            else
            {
                WarrantyNotes = new();
            }
        }

        private void LoadAssemblies()
        {
            var context = new AppDbContext();
            _assemblyService = new AssemblyService(context);

            List<Assembly> all;

            if (_selectedStatusFilter == "Мои")
            {
                // Только сборки, назначенные текущему сборщику
                all = _assemblyService.GetAll()
                    .Where(a => a.AssemblerId == SessionContext.CurrentUser!.UserId)
                    .ToList();
            }
            else if (_selectedStatusFilter == "Все")
            {
                all = _assemblyService.GetAll();
                // Для сборщика: только свои + новые + доработка
                if (SessionContext.IsAssembler)
                    all = all.Where(a =>
                        a.AssemblerId == SessionContext.CurrentUser!.UserId ||
                        a.Status == AssemblyStatus.New ||
                        a.Status == AssemblyStatus.Rework).ToList();
            }
            else
            {
                all = _assemblyService.GetByStatus(_selectedStatusFilter);
                // Для сборщика: ограничиваем видимость даже при выборе статуса
                if (SessionContext.IsAssembler)
                    all = all.Where(a =>
                        a.AssemblerId == SessionContext.CurrentUser!.UserId ||
                        a.Status == AssemblyStatus.New ||
                        a.Status == AssemblyStatus.Rework).ToList();
            }

            if (!string.IsNullOrWhiteSpace(_searchText))
            {
                var q = _searchText.ToLower();
                all = all.Where(a =>
                    a.AssemblyNumber.ToLower().Contains(q) ||
                    a.ClientName.ToLower().Contains(q)).ToList();
            }

            Assemblies = new ObservableCollection<Assembly>(all);
        }

        private void CreateAssembly()
        {
            var dialog = new Views.Dialogs.NewAssemblyDialog();
            if (dialog.ShowDialog() == true)
                LoadAssemblies();
        }

        private void EditAssembly()
        {
            if (SelectedAssembly == null) return;
            var ctx = new AppDbContext();
            var full = ctx.Assemblies
                .Include(a => a.AssemblyComponents)
                    .ThenInclude(ac => ac.Component)
                .FirstOrDefault(a => a.AssemblyId == SelectedAssembly.AssemblyId);
            if (full == null) return;
            var dialog = new Views.Dialogs.NewAssemblyDialog(full);
            if (dialog.ShowDialog() == true)
                LoadAssemblies();
        }

        private void AcceptAssembly()
        {
            if (SelectedAssembly == null) return;
            _assemblyService.AssignAssembler(
                SelectedAssembly.AssemblyId,
                SessionContext.CurrentUser!.UserId);
            LoadAssemblies();
        }

        private void CompleteAssembly()
        {
            if (SelectedAssembly == null) return;
            // Сразу на тестирование, минуя промежуточный статус
            _assemblyService.UpdateStatus(
                SelectedAssembly.AssemblyId,
                AssemblyStatus.OnTesting);
            LoadAssemblies();
        }

        private void SendToTest()
        {
            if (SelectedAssembly == null) return;
            _assemblyService.UpdateStatus(
                SelectedAssembly.AssemblyId,
                AssemblyStatus.OnTesting);
            LoadAssemblies();
        }

        private void TakeRework()
        {
            if (SelectedAssembly == null) return;

            var result = MessageBox.Show(
                $"Взять сборку {SelectedAssembly.AssemblyNumber} на доработку?\n" +
                "Статус изменится на «В сборке».",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            _assemblyService.UpdateStatus(
                SelectedAssembly.AssemblyId,
                AssemblyStatus.InProgress);

            LoadAssemblies();
        }

        private void DeleteAssembly()
        {
            if (SelectedAssembly == null) return;

            var result = MessageBox.Show(
                $"Удалить сборку {SelectedAssembly.AssemblyNumber}?\n" +
                "Все связанные данные будут удалены.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            _assemblyService.Delete(SelectedAssembly.AssemblyId);
            SelectedAssembly = null;
            LoadAssemblies();
        }

        private void PrintAssignment()
        {
            if (SelectedAssembly == null) return;
            var ctx = new AppDbContext();
            var asm = ctx.Assemblies
                .Include(a => a.Assembler)
                .Include(a => a.Dispatcher)
                .Include(a => a.AssemblyComponents)
                    .ThenInclude(ac => ac.Component)
                .FirstOrDefault(a =>
                    a.AssemblyId == SelectedAssembly.AssemblyId);
            if (asm == null) return;
            var pdf = new PdfService().GenerateAssignmentSheet(asm);
            new PdfService().SaveAndOpen(pdf,
                $"ТЗ_{asm.AssemblyNumber}.pdf");
        }

        private void PrintWarranty()
        {
            if (SelectedAssembly == null) return;
            var ctx = new AppDbContext();
            var asm = ctx.Assemblies
                .Include(a => a.AssemblyComponents)
                    .ThenInclude(ac => ac.Component)
                .FirstOrDefault(a =>
                    a.AssemblyId == SelectedAssembly.AssemblyId);
            if (asm == null) return;
            var pdf = new PdfService().GenerateWarrantyCard(asm);
            new PdfService().SaveAndOpen(pdf,
                $"Гарантия_{asm.AssemblyNumber}.pdf");
        }

        private void PrintAcceptance()
        {
            if (SelectedAssembly == null) return;
            var ctx = new AppDbContext();
            var asm = ctx.Assemblies
                .Include(a => a.Assembler)
                .Include(a => a.Dispatcher)
                .Include(a => a.AssemblyComponents)
                    .ThenInclude(ac => ac.Component)
                .Include(a => a.Tests)
                    .ThenInclude(t => t.Tester)
                .FirstOrDefault(a =>
                    a.AssemblyId == SelectedAssembly.AssemblyId);
            if (asm == null) return;
            var lastTest = asm.Tests
                .OrderByDescending(t => t.TestDate)
                .FirstOrDefault();
            if (lastTest == null)
            {
                MessageBox.Show(
                    "Нет данных тестирования для этой сборки.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
            var pdf = new PdfService().GenerateAcceptanceAct(asm, lastTest);
            new PdfService().SaveAndOpen(pdf,
                $"Акт_приёмки_{asm.AssemblyNumber}.pdf");
        }

        private void PrintLabel()
        {
            if (SelectedAssembly == null) return;
            var ctx = new AppDbContext();
            var asm = ctx.Assemblies
                .Include(a => a.Assembler)
                .Include(a => a.AssemblyComponents)
                    .ThenInclude(ac => ac.Component)
                .FirstOrDefault(a =>
                    a.AssemblyId == SelectedAssembly.AssemblyId);
            if (asm == null) return;
            var pdf = new PdfService().GenerateAssemblyLabel(asm);
            new PdfService().SaveAndOpen(pdf,
                $"Этикетка_{asm.AssemblyNumber}.pdf");
        }

        private void ShipSelected()
        {
            var toShip = _selectedAssemblies
                .Where(a => a.Status == AssemblyStatus.Ready)
                .ToList();
            if (toShip.Count == 0) return;

            var list = string.Join("\n", toShip.Select(a =>
                $"  • {a.AssemblyNumber} — {a.ClientName}"));
            var result = MessageBox.Show(
                $"Отметить как отправленные клиенту ({toShip.Count} шт.)?\n\n{list}",
                "Подтверждение отправки",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            var context = new AppDbContext();
            var service = new AssemblyService(context);
            foreach (var a in toShip)
                service.UpdateStatus(a.AssemblyId, AssemblyStatus.Shipped);

            LoadAssemblies();
        }

        private void ShipAllReady()
        {
            var context = new AppDbContext();
            var service = new AssemblyService(context);
            var toShip = service.GetByStatus(AssemblyStatus.Ready);
            if (toShip.Count == 0)
            {
                MessageBox.Show(
                    "Нет сборок со статусом «Готова».",
                    "Нет данных",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var list = string.Join("\n", toShip.Select(a =>
                $"  • {a.AssemblyNumber} — {a.ClientName}"));
            var result = MessageBox.Show(
                $"Отправить все готовые сборки ({toShip.Count} шт.)?\n\n{list}",
                "Подтверждение отправки",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            foreach (var a in toShip)
                service.UpdateStatus(a.AssemblyId, AssemblyStatus.Shipped);

            LoadAssemblies();
        }
    }
}