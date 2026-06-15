using System.Windows;
using МаршрутСборки.Data;
using МаршрутСборки.Models;
using МаршрутСборки.Services;

namespace МаршрутСборки.Views.Dialogs
{
    public partial class ComponentDialog : Window
    {
        private readonly ComponentService _componentService;
        private readonly Component? _existingComponent;

        public ComponentDialog(Component? component = null)
        {
            InitializeComponent();
            _componentService = new ComponentService(new AppDbContext());
            _existingComponent = component;

            if (component != null)
            {
                // Режим редактирования
                TitleText.Text = "Редактировать позицию";
                SubtitleText.Text = component.Name;
                SaveButton.Content = "Сохранить";

                SKUBox.Text = component.SKU;
                NameBox.Text = component.Name;
                CategoryBox.Text = component.Category;
                PriceBox.Text = component.Price.ToString();
                StockBox.Text = component.StockBalance.ToString();
                MinStockBox.Text = component.MinStock.ToString();
            }
            else
            {
                // Режим добавления
                TitleText.Text = "Новая позиция";
                SubtitleText.Text = "Добавить комплектующее в справочник";
                SaveButton.Content = "Добавить";
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SKUBox.Text) ||
                string.IsNullOrWhiteSpace(NameBox.Text) ||
                string.IsNullOrWhiteSpace(CategoryBox.Text))
            {
                MessageBox.Show("Заполните все обязательные поля",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(PriceBox.Text, out decimal price))
                price = 0;

            if (!int.TryParse(StockBox.Text, out int stock))
                stock = 0;

            if (!int.TryParse(MinStockBox.Text, out int minStock))
                minStock = 5;

            if (_existingComponent != null)
            {
                // Редактирование
                _existingComponent.SKU = SKUBox.Text.Trim();
                _existingComponent.Name = NameBox.Text.Trim();
                _existingComponent.Category = CategoryBox.Text.Trim();
                _existingComponent.Price = price;
                _existingComponent.StockBalance = stock;
                _existingComponent.MinStock = minStock;
                _componentService.Update(_existingComponent);
            }
            else
            {
                // Добавление
                _componentService.Create(new Component
                {
                    SKU = SKUBox.Text.Trim(),
                    Name = NameBox.Text.Trim(),
                    Category = CategoryBox.Text.Trim(),
                    Price = price,
                    StockBalance = stock,
                    MinStock = minStock
                });
            }

            DialogResult = true;
        }
    }
}