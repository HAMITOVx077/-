using System.Windows;
using МаршрутСборки.Data;
using МаршрутСборки.Models;
using МаршрутСборки.Services;

namespace МаршрутСборки.Views.Dialogs
{
    public partial class NewUserDialog : Window
    {
        private readonly UserService _userService;

        public NewUserDialog()
        {
            InitializeComponent();
            _userService = new UserService(new AppDbContext());

            var roles = _userService.GetAllRoles();
            RoleBox.ItemsSource = roles;
            RoleBox.SelectedIndex = 0;
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LastNameBox.Text) ||
                string.IsNullOrWhiteSpace(FirstNameBox.Text) ||
                string.IsNullOrWhiteSpace(LoginBox.Text) ||
                string.IsNullOrWhiteSpace(PasswordBox.Password) ||
                RoleBox.SelectedItem == null)
            {
                MessageBox.Show("Заполните все обязательные поля",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var role = (Role)RoleBox.SelectedItem;

            var user = new User
            {
                LastName = LastNameBox.Text.Trim(),
                FirstName = FirstNameBox.Text.Trim(),
                Login = LoginBox.Text.Trim(),
                PasswordHash = PasswordBox.Password,
                RoleId = role.RoleId,
                IsActive = true
            };

            _userService.Create(user);
            DialogResult = true;
        }
    }
}