using System.Windows;
using System.Windows.Input;
using МаршрутСборки.Data;
using МаршрутСборки.Helpers;
using МаршрутСборки.Services;

namespace МаршрутСборки.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private readonly UserService _userService;

        private string _login = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;

        public string Login
        {
            get => _login;
            set => SetProperty(ref _login, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            _userService = new UserService(new AppDbContext());
            LoginCommand = new RelayCommand(
                ExecuteLogin,
                _ => !string.IsNullOrWhiteSpace(Login) && !string.IsNullOrWhiteSpace(Password)
            );
        }

        private void ExecuteLogin(object? parameter)
        {
            ErrorMessage = string.Empty;

            var user = _userService.Authenticate(Login, Password);

            if (user == null)
            {
                ErrorMessage = "Неверный логин или пароль";
                return;
            }

            SessionContext.CurrentUser = user;

            var mainWindow = new Views.MainWindow();
            mainWindow.Show();

            Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w is not Views.MainWindow)
                ?.Close();
        }
    }
}