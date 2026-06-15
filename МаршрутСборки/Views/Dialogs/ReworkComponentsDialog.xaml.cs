using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using МаршрутСборки.Data;
using МаршрутСборки.Helpers;
using МаршрутСборки.Models;
using МаршрутСборки.Services;

namespace МаршрутСборки.Views.Dialogs
{
    public class ReworkItemRow
    {
        public int? OldComponentId { get; set; }
        public int NewComponentId { get; set; }
        public int Quantity { get; set; }
        public string OldName { get; set; } = "— (добавить новый)";
        public string NewName { get; set; } = string.Empty;
    }

    public partial class ReworkComponentsDialog : Window
    {
        private readonly Models.Assembly _assembly;
        private readonly ObservableCollection<ReworkItemRow> _items = new();

        public ReworkComponentsDialog(Models.Assembly assembly)
        {
            InitializeComponent();
            _assembly = assembly;
            TitleText.Text = $"Переделка сборки {assembly.AssemblyNumber} — {assembly.ClientName}";

            CurrentComponentsList.ItemsSource = assembly.AssemblyComponents;

            var context = new AppDbContext();
            var allComponents = context.Components
                .OrderBy(c => c.Category).ThenBy(c => c.Name)
                .ToList();

            // Old component — pick from current assembly components
            OldComponentBox.ItemsSource = assembly.AssemblyComponents.ToList();

            // New component — all available
            NewComponentBox.ItemsSource = allComponents;

            ReworkItemsGrid.ItemsSource = _items;
        }

        private void AddReworkItem_Click(object sender, RoutedEventArgs e)
        {
            if (NewComponentBox.SelectedItem is not Component newComp)
            {
                MessageBox.Show("Выберите новый компонент для установки.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(QuantityBox.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show("Введите корректное количество.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AssemblyComponent? oldAc = OldComponentBox.SelectedItem as AssemblyComponent;

            _items.Add(new ReworkItemRow
            {
                OldComponentId = oldAc?.ComponentId,
                OldName = oldAc?.Component?.Name ?? "— (без замены, добавить)",
                NewComponentId = newComp.ComponentId,
                NewName = newComp.Name,
                Quantity = qty
            });
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (_items.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы одну замену компонента.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var context = new AppDbContext();
            var assemblyService = new AssemblyService(context);
            var notes = NotesBox.Text.Trim();

            foreach (var row in _items)
            {
                context.AssemblyReworkItems.Add(new AssemblyReworkItem
                {
                    AssemblyId = _assembly.AssemblyId,
                    OldComponentId = row.OldComponentId,
                    NewComponentId = row.NewComponentId,
                    Quantity = row.Quantity,
                    Notes = string.IsNullOrWhiteSpace(notes) ? null : notes,
                    IsIssued = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            context.SaveChanges();

            // Set assembly to Rework
            assemblyService.UpdateStatus(_assembly.AssemblyId, AssemblyStatus.Rework);

            var evtLog = new EventLogService(context);
            evtLog.Log(SessionContext.CurrentUser!.UserId, "Назначена переделка",
                $"Сборка {_assembly.AssemblyNumber}: назначена замена {_items.Count} компонент(ов). {notes}");

            DialogResult = true;
        }
    }
}
