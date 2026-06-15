using System.Windows;
using System.Windows.Controls;
using МаршрутСборки.Data;
using МаршрутСборки.Models;
using МаршрутСборки.Services;

namespace МаршрутСборки.Views.Dialogs
{
    public partial class UpdateWarrantyStatusDialog : Window
    {
        private readonly WarrantyCase _case;
        private readonly WarrantyService _warrantyService;

        public UpdateWarrantyStatusDialog(WarrantyCase warrantyCase)
        {
            InitializeComponent();
            _case = warrantyCase;
            _warrantyService = new WarrantyService(new AppDbContext());
            CaseNumberText.Text = $"{warrantyCase.CaseNumber} — {warrantyCase.ClientName}";
            RepairNotesBox.Text = warrantyCase.RepairNotes;

            foreach (ComboBoxItem item in StatusBox.Items)
            {
                if (item.Content.ToString() == warrantyCase.Status)
                {
                    StatusBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (StatusBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите статус",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var status = (StatusBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? string.Empty;
            _warrantyService.UpdateStatus(_case.CaseId, status, RepairNotesBox.Text);
            DialogResult = true;
        }
    }
}