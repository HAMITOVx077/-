using System.Windows.Controls;
using МаршрутСборки.ViewModels;

namespace МаршрутСборки.Views
{
    public partial class ProfileView : UserControl
    {
        public ProfileView()
        {
            InitializeComponent();
        }

        private void CurrentPassword_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is ProfileViewModel vm)
                vm.CurrentPassword = CurrentPasswordBox.Password;
        }

        private void NewPassword_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is ProfileViewModel vm)
                vm.NewPassword = NewPasswordBox.Password;
        }

        private void ConfirmPassword_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is ProfileViewModel vm)
                vm.ConfirmPassword = ConfirmPasswordBox.Password;
        }
    }
}