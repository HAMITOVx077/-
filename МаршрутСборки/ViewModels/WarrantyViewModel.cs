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
    public class WarrantyViewModel : BaseViewModel
    {
        private ObservableCollection<WarrantyCase> _cases = new();
        private WarrantyCase? _selectedCase;

        public ObservableCollection<WarrantyCase> Cases
        {
            get => _cases;
            set => SetProperty(ref _cases, value);
        }

        public WarrantyCase? SelectedCase
        {
            get => _selectedCase;
            set
            {
                SetProperty(ref _selectedCase, value);
                OnPropertyChanged(nameof(CanAssignRework));
                OnPropertyChanged(nameof(CanUpdateStatus));
            }
        }

        public bool CanDelete => SessionContext.IsAdmin;

        public bool CanAssignRework =>
            SelectedCase != null &&
            SelectedCase.Status != WarrantyStatus.InRepair;

        public bool CanUpdateStatus =>
            SelectedCase != null &&
            SelectedCase.Status != WarrantyStatus.InRepair;

        public ICommand LoadCommand { get; }
        public ICommand CreateCommand { get; }
        public ICommand UpdateStatusCommand { get; }
        public ICommand AssignReworkCommand { get; }
        public ICommand DeleteCommand { get; }

        public WarrantyViewModel()
        {
            LoadCommand = new RelayCommand(_ => Load());
            CreateCommand = new RelayCommand(_ => Create());
            UpdateStatusCommand = new RelayCommand(
                _ => UpdateStatus(),
                _ => CanUpdateStatus);
            AssignReworkCommand = new RelayCommand(
                _ => AssignRework(),
                _ => CanAssignRework);
            DeleteCommand = new RelayCommand(
                _ => Delete(),
                _ => SelectedCase != null && CanDelete);
            Load();
        }

        private void Load()
        {
            var selectedId = SelectedCase?.CaseId;
            var context = new AppDbContext();
            var service = new WarrantyService(context);
            Cases = new ObservableCollection<WarrantyCase>(service.GetAll());
            if (selectedId.HasValue)
                SelectedCase = Cases.FirstOrDefault(c => c.CaseId == selectedId.Value);
        }

        private void Create()
        {
            var dialog = new Views.Dialogs.NewWarrantyCaseDialog();
            if (dialog.ShowDialog() == true)
                Load();
        }

        private void UpdateStatus()
        {
            if (SelectedCase == null) return;
            var dialog = new Views.Dialogs.UpdateWarrantyStatusDialog(SelectedCase);
            if (dialog.ShowDialog() == true)
                Load();
        }

        private void AssignRework()
        {
            if (SelectedCase == null) return;

            if (SelectedCase.AssemblyId == null || SelectedCase.Assembly == null)
            {
                MessageBox.Show(
                    "К данному обращению не привязана сборка.\n\n" +
                    "Свяжите обращение со сборкой при создании или обновлении статуса.",
                    "Нет связанной сборки",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Load full assembly with components
            var context = new AppDbContext();
            var assembly = context.Assemblies
                .Include(a => a.AssemblyComponents).ThenInclude(ac => ac.Component)
                .Include(a => a.Assembler)
                .Include(a => a.Dispatcher)
                .FirstOrDefault(a => a.AssemblyId == SelectedCase.Assembly.AssemblyId);

            if (assembly == null) return;

            var dialog = new Views.Dialogs.ReworkComponentsDialog(assembly);
            if (dialog.ShowDialog() == true)
            {
                var warrantyService = new WarrantyService(context);
                warrantyService.UpdateStatus(SelectedCase.CaseId, WarrantyStatus.InRepair,
                    SelectedCase.RepairNotes);
                Load();
            }
        }

        private void Delete()
        {
            if (SelectedCase == null) return;

            var result = MessageBox.Show(
                $"Удалить обращение {SelectedCase.CaseNumber}?\n" +
                $"Клиент: {SelectedCase.ClientName}\n\n" +
                "Это действие нельзя отменить.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            var context = new AppDbContext();
            var service = new WarrantyService(context);
            service.Delete(SelectedCase.CaseId);

            SelectedCase = null;
            Load();
        }
    }
}
