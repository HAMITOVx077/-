using System.Windows;
using МаршрутСборки.Data;
using МаршрутСборки.Helpers;
using МаршрутСборки.Models;
using МаршрутСборки.Services;

namespace МаршрутСборки.Views.Dialogs
{
    public partial class ReceiveDeliveryDialog : Window
    {
        private readonly Component _component;
        private readonly WarehouseService _warehouseService;

        public ReceiveDeliveryDialog(Component component)
        {
            InitializeComponent();
            _component = component;
            _warehouseService = new WarehouseService(new AppDbContext());
            ComponentNameText.Text = component.Name;
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(QuantityBox.Text, out int qty) || qty < 1)
            {
                MessageBox.Show("Введите корректное количество",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _warehouseService.ReceiveDelivery(
                _component.ComponentId,
                qty,
                SessionContext.CurrentUser!.UserId,
                DocumentRefBox.Text
            );

            DialogResult = true;
        }
    }
}