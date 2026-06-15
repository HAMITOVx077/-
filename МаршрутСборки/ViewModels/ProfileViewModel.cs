using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using МаршрутСборки.Data;
using МаршрутСборки.Helpers;
using МаршрутСборки.Services;

namespace МаршрутСборки.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private readonly UserService _userService;

        private string _currentPassword = string.Empty;
        private string _newPassword = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _message = string.Empty;
        private bool _isSuccess;

        public string FullName => SessionContext.CurrentUser?.FullName ?? string.Empty;
        public string Login => SessionContext.CurrentUser?.Login ?? string.Empty;
        public string RoleName => SessionContext.CurrentUser?.Role?.RoleName ?? string.Empty;

        public string OutputDirectoryDisplay =>
            AppSettings.IsCustomDirectory
                ? AppSettings.OutputDirectory
                : $"Рабочий стол ({AppSettings.OutputDirectory})";

        public string CurrentPassword
        {
            get => _currentPassword;
            set => SetProperty(ref _currentPassword, value);
        }

        public string NewPassword
        {
            get => _newPassword;
            set => SetProperty(ref _newPassword, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public bool IsSuccess
        {
            get => _isSuccess;
            set => SetProperty(ref _isSuccess, value);
        }

        public ICommand ChangePasswordCommand { get; }
        public ICommand SetDefaultDirectoryCommand { get; }
        public ICommand ChooseDirectoryCommand { get; }

        public ProfileViewModel()
        {
            _userService = new UserService(new AppDbContext());
            ChangePasswordCommand = new RelayCommand(
                _ => ChangePassword(),
                _ => !string.IsNullOrWhiteSpace(CurrentPassword) &&
                     !string.IsNullOrWhiteSpace(NewPassword) &&
                     !string.IsNullOrWhiteSpace(ConfirmPassword)
            );

            SetDefaultDirectoryCommand = new RelayCommand(_ =>
            {
                AppSettings.OutputDirectory = string.Empty;
                OnPropertyChanged(nameof(OutputDirectoryDisplay));
            });

            ChooseDirectoryCommand = new RelayCommand(_ =>
            {
                var dlg = new OpenFolderDialog
                {
                    Title = "Выберите папку для сохранения документов",
                    InitialDirectory = AppSettings.OutputDirectory
                };
                if (dlg.ShowDialog() == true)
                {
                    AppSettings.OutputDirectory = dlg.FolderName;
                    OnPropertyChanged(nameof(OutputDirectoryDisplay));
                }
            });
        }

        private void ChangePassword()
        {
            Message = string.Empty;

            var user = SessionContext.CurrentUser;
            if (user == null) return;

            if (!BCrypt.Net.BCrypt.Verify(CurrentPassword, user.PasswordHash))
            {
                IsSuccess = false;
                Message = "Текущий пароль введён неверно";
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                IsSuccess = false;
                Message = "Новый пароль и подтверждение не совпадают";
                return;
            }

            if (NewPassword.Length < 6)
            {
                IsSuccess = false;
                Message = "Пароль должен содержать не менее 6 символов";
                return;
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(NewPassword);
            _userService.Update(user);

            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;

            IsSuccess = true;
            Message = "Пароль успешно изменён";
        }
    }
}