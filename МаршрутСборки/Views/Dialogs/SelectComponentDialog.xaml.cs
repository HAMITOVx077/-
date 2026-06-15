using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using МаршрутСборки.Models;

namespace МаршрутСборки.Views.Dialogs
{
    public partial class SelectComponentDialog : Window
    {
        private readonly List<Component> _allComponents;
        public Component? SelectedComponent { get; private set; }
        public int Quantity { get; private set; } = 1;

        public SelectComponentDialog(List<Component> components, List<Component> alreadyAdded)
        {
            InitializeComponent();
            _allComponents = components;
            ComponentsList.ItemsSource = _allComponents;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var query = SearchBox.Text.ToLower();
            ComponentsList.ItemsSource = string.IsNullOrWhiteSpace(query)
                ? _allComponents
                : _allComponents.Where(c =>
                    c.Name.ToLower().Contains(query) ||
                    c.SKU.ToLower().Contains(query) ||
                    c.Category.ToLower().Contains(query)).ToList();
        }

        private void ComponentsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AddButton.IsEnabled = ComponentsList.SelectedItem != null;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            SelectedComponent = ComponentsList.SelectedItem as Component;
            if (!int.TryParse(QuantityBox.Text, out int qty) || qty < 1)
                qty = 1;
            Quantity = qty;
            DialogResult = true;
        }
    }
}