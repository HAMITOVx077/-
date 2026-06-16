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
    public class TestingViewModel : BaseViewModel
    {
        private ObservableCollection<Assembly> _assemblies = new();
        private ObservableCollection<Assembly> _completedAssemblies = new();
        private ObservableCollection<AssemblyReworkItem> _allReworkItems = new();
        private Assembly? _selectedAssembly;
        private Assembly? _selectedCompletedAssembly;

        public ObservableCollection<Assembly> Assemblies
        {
            get => _assemblies;
            set => SetProperty(ref _assemblies, value);
        }

        public ObservableCollection<Assembly> CompletedAssemblies
        {
            get => _completedAssemblies;
            set => SetProperty(ref _completedAssemblies, value);
        }

        public ObservableCollection<AssemblyReworkItem> AllReworkItems
        {
            get => _allReworkItems;
            set
            {
                SetProperty(ref _allReworkItems, value);
                OnPropertyChanged(nameof(HasReworkHistory));
            }
        }

        public bool HasReworkHistory => _allReworkItems.Count > 0;

        public Assembly? SelectedAssembly
        {
            get => _selectedAssembly;
            set
            {
                if (SetProperty(ref _selectedAssembly, value))
                    LoadReworkItems();
            }
        }

        public Assembly? SelectedCompletedAssembly
        {
            get => _selectedCompletedAssembly;
            set => SetProperty(ref _selectedCompletedAssembly, value);
        }

        public ICommand SaveResultCommand { get; }
        public ICommand PrintTestActCommand { get; }
        public ICommand PrintTestActCompletedCommand { get; }

        public TestingViewModel()
        {
            SaveResultCommand = new RelayCommand(
                _ => SaveResult(),
                _ => SelectedAssembly != null && SessionContext.IsTester);

            PrintTestActCommand = new RelayCommand(
                _ => PrintTestAct(SelectedAssembly),
                _ => SelectedAssembly != null);

            PrintTestActCompletedCommand = new RelayCommand(
                _ => PrintTestAct(SelectedCompletedAssembly),
                _ => SelectedCompletedAssembly != null);

            Load();
        }

        private void LoadReworkItems()
        {
            if (SelectedAssembly == null)
            {
                AllReworkItems = new();
                return;
            }
            var context = new AppDbContext();
            var items = context.AssemblyReworkItems
                .Include(r => r.OldComponent)
                .Include(r => r.NewComponent)
                .Where(r => r.AssemblyId == SelectedAssembly.AssemblyId)
                .OrderBy(r => r.CreatedAt)
                .ToList();
            AllReworkItems = new ObservableCollection<AssemblyReworkItem>(items);
        }

        private void Load()
        {
            var context = new AppDbContext();

            var forTesting = context.Assemblies
                .Include(a => a.Assembler)
                .Include(a => a.Dispatcher)
                .Include(a => a.AssemblyComponents)
                    .ThenInclude(ac => ac.Component)
                .Include(a => a.Tests)
                    .ThenInclude(t => t.Tester)
                .Where(a =>
                    a.Status == AssemblyStatus.ReadyForTest ||
                    a.Status == AssemblyStatus.OnTesting)
                .OrderByDescending(a => a.CreationDate)
                .ToList();

            Assemblies = new ObservableCollection<Assembly>(forTesting);

            var completed = context.Assemblies
                .Include(a => a.Assembler)
                .Include(a => a.Dispatcher)
                .Include(a => a.AssemblyComponents)
                    .ThenInclude(ac => ac.Component)
                .Include(a => a.Tests)
                    .ThenInclude(t => t.Tester)
                .Where(a =>
                    a.Status == AssemblyStatus.Ready ||
                    a.Status == AssemblyStatus.Rework ||
                    a.Status == AssemblyStatus.Shipped)
                .Where(a => a.Tests.Any())
                .OrderByDescending(a => a.CreationDate)
                .Take(50)
                .ToList();

            CompletedAssemblies =
                new ObservableCollection<Assembly>(completed);
        }

        private void SaveResult()
        {
            if (SelectedAssembly == null) return;

            var dialog = new Views.Dialogs.TestResultDialog(
                SelectedAssembly);
            if (dialog.ShowDialog() == true)
                Load();
        }

        private void PrintTestAct(Assembly? assembly)
        {
            if (assembly == null) return;

            var context = new AppDbContext();
            var asm = context.Assemblies
                .Include(a => a.Assembler)
                .Include(a => a.AssemblyComponents)
                    .ThenInclude(ac => ac.Component)
                .Include(a => a.Tests)
                    .ThenInclude(t => t.Tester)
                .FirstOrDefault(a => a.AssemblyId == assembly.AssemblyId);

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

            var pdf = new PdfService().GenerateTestAct(lastTest, asm);
            new PdfService().SaveAndOpen(pdf,
                $"Акт_тестирования_{asm.AssemblyNumber}.pdf");
        }
    }
}