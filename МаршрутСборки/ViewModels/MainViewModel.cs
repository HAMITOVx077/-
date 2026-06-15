using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using МаршрутСборки.Helpers;

namespace МаршрутСборки.ViewModels
{
    public class MenuItem : BaseViewModel
    {
        private bool _isSelected;

        public string Title { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public BaseViewModel ViewModel { get; set; } = null!;

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }

    public class MainViewModel : BaseViewModel
    {
        private BaseViewModel _currentViewModel = null!;
        private MenuItem? _selectedMenuItem;

        public BaseViewModel CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public MenuItem? SelectedMenuItem
        {
            get => _selectedMenuItem;
            set
            {
                if (SetProperty(ref _selectedMenuItem, value) && value != null)
                    CurrentViewModel = value.ViewModel;
            }
        }

        public ObservableCollection<MenuItem> MenuItems { get; } = new();

        public string UserFullName => SessionContext.CurrentUser?.FullName ?? string.Empty;
        public string UserRole => SessionContext.CurrentUser?.Role?.RoleName ?? string.Empty;

        public ICommand LogoutCommand { get; }
        public ICommand SelectMenuItemCommand { get; }

        public MainViewModel()
        {
            LogoutCommand = new RelayCommand(_ => Logout());

            SelectMenuItemCommand = new RelayCommand(item =>
            {
                if (item is MenuItem menuItem)
                {
                    foreach (var m in MenuItems)
                        m.IsSelected = false;
                    menuItem.IsSelected = true;
                    SelectedMenuItem = menuItem;
                }
            });

            BuildMenu();

            if (MenuItems.Any())
            {
                MenuItems.First().IsSelected = true;
                SelectedMenuItem = MenuItems.First();
            }
        }

        private void BuildMenu()
        {
            var user = SessionContext.CurrentUser;
            if (user == null) return;

            var role = user.Role.RoleName;

            // Профиль — первый пункт для всех
            MenuItems.Add(new MenuItem
            {
                Title = "Профиль",
                Icon = "👤",
                ViewModel = new ProfileViewModel()
            });

            // Сборки — для всех (сборщик видит только свои)
            MenuItems.Add(new MenuItem
            {
                Title = "Сборки",
                Icon = "🔧",
                ViewModel = new AssembliesViewModel()
            });

            // Склад — кладовщик и администратор
            if (role is "Кладовщик" or "Администратор")
                MenuItems.Add(new MenuItem
                {
                    Title = "Склад",
                    Icon = "📦",
                    ViewModel = new WarehouseViewModel()
                });

            // Тестирование — тестировщик и администратор
            if (role is "Тестировщик" or "Администратор")
                MenuItems.Add(new MenuItem
                {
                    Title = "Тестирование",
                    Icon = "✅",
                    ViewModel = new TestingViewModel()
                });

            // Гарантия — гарантийный инженер и администратор
            if (role is "Гарантийный инженер" or "Администратор")
                MenuItems.Add(new MenuItem
                {
                    Title = "Гарантия",
                    Icon = "🛡",
                    ViewModel = new WarrantyViewModel()
                });

            // Только администратор
            if (role == "Администратор")
            {
                MenuItems.Add(new MenuItem
                {
                    Title = "Сотрудники",
                    Icon = "👥",
                    ViewModel = new UsersViewModel()
                });
                MenuItems.Add(new MenuItem
                {
                    Title = "Аналитика",
                    Icon = "📊",
                    ViewModel = new ReportsViewModel()
                });
            }
        }

        private void Logout()
        {
            SessionContext.Clear();
            var loginWindow = new Views.LoginWindow();
            loginWindow.Show();
            Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w is not Views.LoginWindow)
                ?.Close();
        }
    }
}