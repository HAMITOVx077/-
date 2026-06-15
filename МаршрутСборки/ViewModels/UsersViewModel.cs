using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using МаршрутСборки.Data;
using МаршрутСборки.Helpers;
using МаршрутСборки.Models;
using МаршрутСборки.Services;

namespace МаршрутСборки.ViewModels
{
    public class UsersViewModel : BaseViewModel
    {
        private readonly UserService _userService;

        private ObservableCollection<User> _users = new();
        private User? _selectedUser;

        public ObservableCollection<User> Users
        {
            get => _users;
            set => SetProperty(ref _users, value);
        }

        public User? SelectedUser
        {
            get => _selectedUser;
            set => SetProperty(ref _selectedUser, value);
        }

        public ICommand LoadCommand { get; }
        public ICommand CreateCommand { get; }
        public ICommand DeactivateCommand { get; }
        public ICommand ResetPasswordCommand { get; }

        public UsersViewModel()
        {
            _userService = new UserService(new AppDbContext());
            LoadCommand = new RelayCommand(_ => Load());
            CreateCommand = new RelayCommand(_ => Create());
            DeactivateCommand = new RelayCommand(
                _ => Deactivate(),
                _ => SelectedUser != null &&
                     SelectedUser.UserId != SessionContext.CurrentUser!.UserId);
            ResetPasswordCommand = new RelayCommand(
                _ => ResetPassword(),
                _ => SelectedUser != null);
            Load();
        }

        private void Load()
        {
            var selected = SelectedUser;
            Users = new ObservableCollection<User>(_userService.GetAll());
            if (selected != null)
                SelectedUser = Users.FirstOrDefault(u => u.UserId == selected.UserId);
        }

        private void Create()
        {
            var dialog = new Views.Dialogs.NewUserDialog();
            if (dialog.ShowDialog() == true)
                Load();
        }

        private void Deactivate()
        {
            if (SelectedUser == null) return;

            var result = MessageBox.Show(
                $"Деактивировать аккаунт {SelectedUser.FullName}?\n" +
                "Сотрудник потеряет доступ к системе.",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            _userService.Deactivate(SelectedUser.UserId);
            SelectedUser = null;
            Load();
        }

        private void ResetPassword()
        {
            if (SelectedUser == null) return;

            var defaultPassword = "123456";
            SelectedUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(defaultPassword);
            _userService.Update(SelectedUser);

            MessageBox.Show(
                $"Пароль сотрудника {SelectedUser.FullName} сброшен.\n" +
                $"Новый пароль: {defaultPassword}",
                "Пароль сброшен",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}