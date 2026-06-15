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
    public class AssemblyComponentItem : BaseViewModel
    {
        private int _quantity;

        public Component Component { get; set; } = null!;

        public int Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }
    }

    public class NewAssemblyViewModel : BaseViewModel
    {
        private readonly AssemblyService _assemblyService;
        private readonly ComponentService _componentService;
        private readonly int? _editAssemblyId;

        private string _clientName = string.Empty;
        private string _notes = string.Empty;
        private string _errorMessage = string.Empty;
        private DateTime? _deadline;
        private string _selectedPriority = AssemblyPriority.Medium;
        private int _quantity = 1;

        public bool IsEditMode => _editAssemblyId.HasValue;
        public string DialogTitle => IsEditMode ? "Редактировать сборку" : "Новая сборка";
        public string DialogSubtitle => IsEditMode ? "Изменение данных заказа" : "Заполните данные заказа";
        public string ConfirmButtonText => IsEditMode ? "Сохранить" : "Создать сборку";
        public bool ShowQuantity => !IsEditMode;

        public string ClientName
        {
            get => _clientName;
            set => SetProperty(ref _clientName, value);
        }

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public DateTime? Deadline
        {
            get => _deadline;
            set => SetProperty(ref _deadline, value);
        }

        public string SelectedPriority
        {
            get => _selectedPriority;
            set => SetProperty(ref _selectedPriority, value);
        }

        public int Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, Math.Max(1, Math.Min(500, value)));
        }

        public List<string> Priorities { get; } = new()
        {
            AssemblyPriority.Low,
            AssemblyPriority.Medium,
            AssemblyPriority.High,
            AssemblyPriority.Critical
        };

        public ObservableCollection<AssemblyComponentItem> SelectedComponents { get; } = new();

        public ICommand CreateCommand { get; }
        public ICommand AddComponentCommand { get; }
        public ICommand RemoveComponentCommand { get; }
        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }

        public NewAssemblyViewModel() : this(null) { }

        public NewAssemblyViewModel(Models.Assembly? existing)
        {
            var context = new AppDbContext();
            _assemblyService = new AssemblyService(context);
            _componentService = new ComponentService(context);

            if (existing != null)
            {
                _editAssemblyId = existing.AssemblyId;
                _clientName = existing.ClientName;
                _notes = existing.Notes ?? string.Empty;
                _deadline = existing.Deadline?.ToLocalTime();
                _selectedPriority = existing.Priority;

                foreach (var ac in existing.AssemblyComponents)
                    SelectedComponents.Add(new AssemblyComponentItem
                    {
                        Component = ac.Component,
                        Quantity = ac.Quantity
                    });
            }

            CreateCommand = new RelayCommand(_ => Create(), _ => CanCreate());
            AddComponentCommand = new RelayCommand(_ => AddComponent());
            RemoveComponentCommand = new RelayCommand(item =>
            {
                if (item is AssemblyComponentItem comp)
                    SelectedComponents.Remove(comp);
            });
            IncreaseQuantityCommand = new RelayCommand(_ => Quantity++);
            DecreaseQuantityCommand = new RelayCommand(_ => Quantity--);
        }

        private bool CanCreate()
        {
            return !string.IsNullOrWhiteSpace(ClientName) &&
                   Deadline.HasValue;
        }

        private void AddComponent()
        {
            var dialog = new Views.Dialogs.SelectComponentDialog(
                _componentService.GetAll(),
                SelectedComponents.Select(c => c.Component).ToList());

            if (dialog.ShowDialog() == true && dialog.SelectedComponent != null)
            {
                var existing = SelectedComponents
                    .FirstOrDefault(c => c.Component.ComponentId == dialog.SelectedComponent.ComponentId);

                if (existing != null)
                    existing.Quantity += dialog.Quantity;
                else
                    SelectedComponents.Add(new AssemblyComponentItem
                    {
                        Component = dialog.SelectedComponent,
                        Quantity = dialog.Quantity
                    });
            }
        }

        private static readonly string[] RequiredCategories =
        {
            "Процессор", "Оперативная память", "Накопитель",
            "Материнская плата", "Корпус", "Блок питания"
        };

        private void Create()
        {
            ErrorMessage = string.Empty;

            if (!Deadline.HasValue)
            {
                ErrorMessage = "Укажите срок выполнения";
                return;
            }

            if (!IsEditMode && Deadline.Value.Date < DateTime.Today)
            {
                ErrorMessage = $"Срок не может быть в прошлом (сегодня {DateTime.Today:dd.MM.yyyy})";
                return;
            }

            var presentCategories = SelectedComponents
                .Select(c => c.Component.Category)
                .ToHashSet();

            var missing = RequiredCategories
                .Where(cat => !presentCategories.Contains(cat))
                .ToList();

            if (missing.Count > 0)
            {
                ErrorMessage = "Минимальная комплектация не соблюдена. Добавьте: " +
                               string.Join(", ", missing);
                return;
            }

            var components = SelectedComponents.Select(c => new AssemblyComponent
            {
                ComponentId = c.Component.ComponentId,
                Quantity = c.Quantity
            }).ToList();

            if (IsEditMode)
            {
                _assemblyService.Update(
                    _editAssemblyId!.Value,
                    ClientName, Notes,
                    Deadline?.ToUniversalTime(),
                    SelectedPriority, components);
            }
            else
            {
                var template = new Models.Assembly
                {
                    ClientName = ClientName,
                    Notes = Notes,
                    Deadline = Deadline?.ToUniversalTime(),
                    Priority = SelectedPriority,
                    DispatcherId = SessionContext.CurrentUser!.UserId,
                    Status = AssemblyStatus.New
                };
                _assemblyService.CreateBatch(template, components, Quantity);
            }

            foreach (Window w in Application.Current.Windows)
            {
                if (w is Views.Dialogs.NewAssemblyDialog d)
                {
                    d.DialogResult = true;
                    return;
                }
            }
        }
    }
}