using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using МаршрутСборки.Data;
using МаршрутСборки.Helpers;
using МаршрутСборки.Models;
using МаршрутСборки.Services;

namespace МаршрутСборки.ViewModels
{
    public class WarehouseViewModel : BaseViewModel
    {
        private readonly ComponentService _componentService;
        private readonly WarehouseService _warehouseService;

        private ObservableCollection<Component> _components = new();
        private ObservableCollection<WarehouseOperation> _operations = new();
        private ObservableCollection<Models.Assembly> _pendingAssemblies = new();
        private ObservableCollection<Models.Assembly> _reworkAssemblies = new();
        private ObservableCollection<AssemblyReworkItem> _selectedReworkItems = new();
        private Component? _selectedComponent;
        private Models.Assembly? _selectedPendingAssembly;
        private Models.Assembly? _selectedReworkAssembly;
        private string _searchText = string.Empty;

        public ObservableCollection<Component> Components
        {
            get => _components;
            set => SetProperty(ref _components, value);
        }

        public ObservableCollection<WarehouseOperation> Operations
        {
            get => _operations;
            set => SetProperty(ref _operations, value);
        }

        public ObservableCollection<Models.Assembly> PendingAssemblies
        {
            get => _pendingAssemblies;
            set => SetProperty(ref _pendingAssemblies, value);
        }

        public ObservableCollection<Models.Assembly> ReworkAssemblies
        {
            get => _reworkAssemblies;
            set => SetProperty(ref _reworkAssemblies, value);
        }

        public ObservableCollection<AssemblyReworkItem> SelectedReworkItems
        {
            get => _selectedReworkItems;
            set => SetProperty(ref _selectedReworkItems, value);
        }

        public Component? SelectedComponent
        {
            get => _selectedComponent;
            set => SetProperty(ref _selectedComponent, value);
        }

        public Models.Assembly? SelectedPendingAssembly
        {
            get => _selectedPendingAssembly;
            set => SetProperty(ref _selectedPendingAssembly, value);
        }

        public Models.Assembly? SelectedReworkAssembly
        {
            get => _selectedReworkAssembly;
            set
            {
                SetProperty(ref _selectedReworkAssembly, value);
                LoadReworkItems();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    LoadComponents();
            }
        }

        public int LowStockCount => _components.Count(c => c.IsLowStock);
        public bool HasLowStock => LowStockCount > 0;
        public int ReworkCount => _reworkAssemblies.Count;
        public bool HasRework => ReworkCount > 0;

        public ICommand LoadCommand { get; }
        public ICommand AddComponentCommand { get; }
        public ICommand EditComponentCommand { get; }
        public ICommand ReceiveDeliveryCommand { get; }
        public ICommand IssueComponentsCommand { get; }
        public ICommand PrintIssueActCommand { get; }
        public ICommand IssueReworkCommand { get; }
        public ICommand PrintReworkActCommand { get; }

        public WarehouseViewModel()
        {
            var context = new AppDbContext();
            _componentService = new ComponentService(context);
            _warehouseService = new WarehouseService(context);

            LoadCommand = new RelayCommand(_ => { LoadComponents(); LoadOperations(); LoadPendingAssemblies(); LoadReworkAssemblies(); });
            AddComponentCommand = new RelayCommand(_ => AddComponent());
            EditComponentCommand = new RelayCommand(_ => EditComponent(), _ => SelectedComponent != null);
            ReceiveDeliveryCommand = new RelayCommand(_ => ReceiveDelivery(), _ => SelectedComponent != null);
            IssueComponentsCommand = new RelayCommand(_ => IssueComponents(), _ => SelectedPendingAssembly != null);
            PrintIssueActCommand = new RelayCommand(_ => PrintIssueAct(), _ => SelectedPendingAssembly != null);
            IssueReworkCommand = new RelayCommand(_ => IssueReworkComponents(), _ => SelectedReworkAssembly != null);
            PrintReworkActCommand = new RelayCommand(_ => PrintReworkAct(), _ => SelectedReworkAssembly != null);

            LoadComponents();
            LoadOperations();
            LoadPendingAssemblies();
            LoadReworkAssemblies();
        }

        private void PrintIssueAct()
        {
            if (SelectedPendingAssembly == null) return;

            var context = new AppDbContext();
            var assembly = context.Assemblies
                .Include(a => a.Assembler)
                .Include(a => a.Dispatcher)
                .Include(a => a.AssemblyComponents).ThenInclude(ac => ac.Component)
                .FirstOrDefault(a => a.AssemblyId == SelectedPendingAssembly.AssemblyId);

            if (assembly == null) return;

            var pdfService = new PdfService();
            var pdf = pdfService.GenerateIssueAct(assembly, SessionContext.CurrentUser!);
            pdfService.SaveAndOpen(pdf, $"Акт_выдачи_{assembly.AssemblyNumber}.pdf");
        }

        private void LoadComponents()
        {
            // Свежий контекст чтобы не читать из кэша EF
            var freshService = new ComponentService(new AppDbContext());
            var list = string.IsNullOrWhiteSpace(SearchText)
                ? freshService.GetAll()
                : freshService.Search(SearchText);

            Components = new ObservableCollection<Component>(list);
            OnPropertyChanged(nameof(LowStockCount));
            OnPropertyChanged(nameof(HasLowStock));
        }

        private void LoadOperations()
        {
            var ops = _warehouseService.GetHistory();
            Operations = new ObservableCollection<WarehouseOperation>(ops);
        }

        private void LoadPendingAssemblies()
        {
            var context = new AppDbContext();

            // Explicitly use int (not int?) to avoid nullable comparison issues
            var issuedAssemblyIds = context.WarehouseOperations
                .Where(o => o.OperationType == OperationType.Issue && o.AssemblyId != null)
                .Select(o => o.AssemblyId!.Value)
                .Distinct()
                .ToList();

            var list = context.Assemblies
                .Include(a => a.Assembler)
                .Include(a => a.Dispatcher)
                .Where(a => a.Status == AssemblyStatus.InProgress &&
                             !issuedAssemblyIds.Contains(a.AssemblyId))
                .ToList();

            PendingAssemblies = new ObservableCollection<Models.Assembly>(list);
        }

        private void LoadReworkAssemblies()
        {
            var context = new AppDbContext();
            var svc = new WarehouseService(context);
            var list = svc.GetReworkAssemblies();
            ReworkAssemblies = new ObservableCollection<Models.Assembly>(list);
            OnPropertyChanged(nameof(ReworkCount));
            OnPropertyChanged(nameof(HasRework));
        }

        private void LoadReworkItems()
        {
            if (SelectedReworkAssembly == null)
            {
                SelectedReworkItems = new();
                return;
            }

            var context = new AppDbContext();
            var svc = new WarehouseService(context);
            var items = svc.GetPendingReworkItems(SelectedReworkAssembly.AssemblyId);
            SelectedReworkItems = new ObservableCollection<AssemblyReworkItem>(items);
        }

        private void IssueReworkComponents()
        {
            if (SelectedReworkAssembly == null) return;

            var pendingItems = SelectedReworkItems.ToList();
            if (pendingItems.Count == 0)
            {
                MessageBox.Show("Нет позиций для выдачи.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var itemDescriptions = string.Join("\n",
                pendingItems.Select(i =>
                    $"  — {i.OldComponent?.Name ?? "(без замены)"} → {i.NewComponent?.Name} ×{i.Quantity}"));

            var result = MessageBox.Show(
                $"Выдать замену для сборки {SelectedReworkAssembly.AssemblyNumber}?\n\n" +
                itemDescriptions +
                "\n\nНовые детали спишутся, старые — вернутся на склад.\n" +
                "Сборка перейдёт в статус «В сборке».",
                "Подтверждение выдачи по переделке",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            // Load full assembly for the act BEFORE marking IsIssued
            var ctxForPrint = new AppDbContext();
            var assemblyForPrint = ctxForPrint.Assemblies
                .Include(a => a.Assembler).Include(a => a.Dispatcher)
                .Include(a => a.AssemblyComponents).ThenInclude(ac => ac.Component)
                .FirstOrDefault(a => a.AssemblyId == SelectedReworkAssembly.AssemblyId);

            var assemblyNumber = SelectedReworkAssembly.AssemblyNumber;

            // Execute the issue
            var context = new AppDbContext();
            var svc = new WarehouseService(context);
            List<AssemblyReworkItem> issuedItems;
            try
            {
                issuedItems = svc.IssueReworkComponents(
                    SelectedReworkAssembly.AssemblyId, SessionContext.CurrentUser!.UserId);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Недостаточно запасов",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedReworkAssembly = null;
            LoadComponents();
            LoadOperations();
            LoadPendingAssemblies();
            LoadReworkAssemblies();

            // Offer to print the rework act
            if (assemblyForPrint != null && issuedItems.Count > 0)
            {
                var printAsk = MessageBox.Show(
                    $"Замена выдана. Распечатать акт переделки?",
                    "Готово",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (printAsk == MessageBoxResult.Yes)
                {
                    var pdfService = new PdfService();
                    var pdf = pdfService.GenerateReworkIssueAct(
                        assemblyForPrint, issuedItems, SessionContext.CurrentUser!);
                    pdfService.SaveAndOpen(pdf, $"Акт_переделки_{assemblyNumber}.pdf");
                }
            }
            else
            {
                MessageBox.Show("Замена выдана. Состав сборки обновлён.",
                    "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void PrintReworkAct()
        {
            if (SelectedReworkAssembly == null) return;

            // Print pending items (before issue)
            var context = new AppDbContext();
            var assembly = context.Assemblies
                .Include(a => a.Assembler).Include(a => a.Dispatcher)
                .Include(a => a.AssemblyComponents).ThenInclude(ac => ac.Component)
                .FirstOrDefault(a => a.AssemblyId == SelectedReworkAssembly.AssemblyId);

            if (assembly == null) return;

            var items = SelectedReworkItems.ToList();
            if (items.Count == 0)
            {
                MessageBox.Show("Нет позиций для печати акта.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var pdfService = new PdfService();
            var pdf = pdfService.GenerateReworkIssueAct(assembly, items, SessionContext.CurrentUser!);
            pdfService.SaveAndOpen(pdf, $"Акт_переделки_{assembly.AssemblyNumber}.pdf");
        }

        private void AddComponent()
        {
            var dialog = new Views.Dialogs.ComponentDialog(null);
            if (dialog.ShowDialog() == true)
                LoadComponents();
        }

        private void EditComponent()
        {
            if (SelectedComponent == null) return;
            var dialog = new Views.Dialogs.ComponentDialog(SelectedComponent);
            if (dialog.ShowDialog() == true)
                LoadComponents();
        }

        private void ReceiveDelivery()
        {
            if (SelectedComponent == null) return;
            var dialog = new Views.Dialogs.ReceiveDeliveryDialog(SelectedComponent);
            if (dialog.ShowDialog() == true)
            {
                LoadComponents();
                LoadOperations();
            }
        }

        private void IssueComponents()
        {
            if (SelectedPendingAssembly == null) return;

            var result = MessageBox.Show(
                $"Выдать комплектующие для сборки {SelectedPendingAssembly.AssemblyNumber}?\n" +
                $"Клиент: {SelectedPendingAssembly.ClientName}\n\n" +
                "Комплектующие будут списаны со склада.",
                "Подтверждение выдачи",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            // Save assembly info before clearing selection
            var assemblyId = SelectedPendingAssembly.AssemblyId;
            var assemblyNumber = SelectedPendingAssembly.AssemblyNumber;

            try
            {
                _warehouseService.IssueComponents(
                    assemblyId,
                    SessionContext.CurrentUser!.UserId,
                    $"Выдача под сборку {assemblyNumber}");
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Недостаточно запасов",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedPendingAssembly = null;
            LoadComponents();
            LoadOperations();
            LoadPendingAssemblies();

            // Auto-print issue act
            var ctx = new AppDbContext();
            var assembly = ctx.Assemblies
                .Include(a => a.Assembler)
                .Include(a => a.Dispatcher)
                .Include(a => a.AssemblyComponents).ThenInclude(ac => ac.Component)
                .FirstOrDefault(a => a.AssemblyId == assemblyId);

            if (assembly != null)
            {
                var pdfService = new PdfService();
                var pdf = pdfService.GenerateIssueAct(assembly, SessionContext.CurrentUser!);
                pdfService.SaveAndOpen(pdf, $"Акт_выдачи_{assemblyNumber}.pdf");
            }
        }
    }
}
