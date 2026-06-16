using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using МаршрутСборки.Data;
using МаршрутСборки.Helpers;
using МаршрутСборки.Models;
using МаршрутСборки.Services;
using System;

namespace МаршрутСборки.ViewModels
{
    public class WarrantyViewModel : BaseViewModel
    {
        private ObservableCollection<WarrantyCase> _cases = new();
        private WarrantyCase? _selectedCase;
        private ObservableCollection<WarrantyCaseNote> _notes = new();
        private string _newNoteText = string.Empty;

        public ObservableCollection<WarrantyCase> Cases
        {
            get => _cases;
            set => SetProperty(ref _cases, value);
        }

        public ObservableCollection<WarrantyCaseNote> Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        public string NewNoteText
        {
            get => _newNoteText;
            set
            {
                if (SetProperty(ref _newNoteText, value))
                    OnPropertyChanged(nameof(CanAddNote));
            }
        }

        public WarrantyCase? SelectedCase
        {
            get => _selectedCase;
            set
            {
                SetProperty(ref _selectedCase, value);
                OnPropertyChanged(nameof(CanAssignRework));
                OnPropertyChanged(nameof(AssignReworkLabel));
                OnPropertyChanged(nameof(CanCloseCase));
                OnPropertyChanged(nameof(CanAddNote));
                LoadNotes();
            }
        }

        public bool CanDelete => SessionContext.IsWarrantyEngineer;

        public bool CanAssignRework =>
            SessionContext.IsWarrantyEngineer &&
            SelectedCase != null &&
            SelectedCase.Status != WarrantyStatus.ReadyForPickup &&
            SelectedCase.Status != WarrantyStatus.Closed;

        public string AssignReworkLabel =>
            SelectedCase?.Status == WarrantyStatus.InRepair
                ? "➕ Добавить замену деталей"
                : "🔧 Назначить переделку";

        public bool CanAddNote =>
            SessionContext.IsWarrantyEngineer &&
            SelectedCase != null &&
            SelectedCase.Status != WarrantyStatus.ReadyForPickup &&
            SelectedCase.Status != WarrantyStatus.Closed;

        public bool CanCloseCase =>
            SessionContext.IsWarrantyEngineer &&
            SelectedCase != null &&
            SelectedCase.Status == WarrantyStatus.ReadyForPickup;

        public ICommand LoadCommand { get; }
        public ICommand CreateCommand { get; }
        public ICommand AssignReworkCommand { get; }
        public ICommand CloseCaseCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand AddNoteCommand { get; }

        public WarrantyViewModel()
        {
            LoadCommand       = new RelayCommand(_ => Load());
            CreateCommand     = new RelayCommand(_ => Create(), _ => SessionContext.IsWarrantyEngineer);
            AssignReworkCommand = new RelayCommand(_ => AssignRework(), _ => CanAssignRework);
            CloseCaseCommand  = new RelayCommand(_ => CloseCase(), _ => CanCloseCase);
            DeleteCommand     = new RelayCommand(
                _ => Delete(),
                _ => SelectedCase != null && CanDelete);
            AddNoteCommand = new RelayCommand(_ => AddNote(), _ => CanAddNote);
            Load();
        }

        private void Load()
        {
            var selectedId = SelectedCase?.CaseId;
            var context = new AppDbContext();
            var service = new WarrantyService(context);
            var cases = service.GetAll();

            // Авто-переход: если сборка прошла ремонт и стала Готова/Отгружена → Готово к выдаче
            foreach (var c in cases.Where(c =>
                c.Status == WarrantyStatus.InRepair &&
                c.Assembly != null &&
                (c.Assembly.Status == AssemblyStatus.Ready ||
                 c.Assembly.Status == AssemblyStatus.Shipped)))
            {
                service.UpdateStatus(c.CaseId, WarrantyStatus.ReadyForPickup, c.RepairNotes);
                c.Status = WarrantyStatus.ReadyForPickup;
            }

            // Авто-закрытие: если сборка отгружена → обращение закрыто
            foreach (var c in cases.Where(c =>
                c.Status == WarrantyStatus.ReadyForPickup &&
                c.Assembly?.Status == AssemblyStatus.Shipped))
            {
                service.UpdateStatus(c.CaseId, WarrantyStatus.Closed, c.RepairNotes);
                c.Status = WarrantyStatus.Closed;
            }

            Cases = new ObservableCollection<WarrantyCase>(cases);
            if (selectedId.HasValue)
                SelectedCase = Cases.FirstOrDefault(c => c.CaseId == selectedId.Value);
        }

        private void Create()
        {
            var dialog = new Views.Dialogs.NewWarrantyCaseDialog();
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
                    "Укажите серийный номер сборки при создании обращения.",
                    "Нет связанной сборки",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

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

        private void CloseCase()
        {
            if (SelectedCase == null) return;

            var result = MessageBox.Show(
                $"Подтвердить выдачу клиенту?\n\n" +
                $"Обращение {SelectedCase.CaseNumber} будет закрыто.\n" +
                $"Клиент: {SelectedCase.ClientName}",
                "Выдача клиенту",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            var ctx = new AppDbContext();
            new WarrantyService(ctx).UpdateStatus(SelectedCase.CaseId, WarrantyStatus.Closed);
            Load();
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
            new WarrantyService(context).Delete(SelectedCase.CaseId);
            SelectedCase = null;
            Load();
        }

        private void LoadNotes()
        {
            if (SelectedCase == null) { Notes = new(); return; }
            var context = new AppDbContext();
            var items = context.WarrantyCaseNotes
                .Include(n => n.Author)
                .Where(n => n.CaseId == SelectedCase.CaseId)
                .OrderBy(n => n.CreatedAt)
                .ToList();
            Notes = new ObservableCollection<WarrantyCaseNote>(items);
        }

        private void AddNote()
        {
            if (SelectedCase == null || string.IsNullOrWhiteSpace(_newNoteText)) return;
            var context = new AppDbContext();
            var note = new WarrantyCaseNote
            {
                CaseId = SelectedCase.CaseId,
                Text = _newNoteText.Trim(),
                CreatedAt = DateTime.UtcNow,
                AuthorId = SessionContext.CurrentUser!.UserId
            };
            context.WarrantyCaseNotes.Add(note);
            context.SaveChanges();
            NewNoteText = string.Empty;
            LoadNotes();
        }
    }
}
