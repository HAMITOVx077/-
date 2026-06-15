using System.Windows;
using МаршрутСборки.Helpers;
using МаршрутСборки.ViewModels;

namespace МаршрутСборки.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        public class MenuItem : BaseViewModel
        {
            public string Title { get; set; } = string.Empty;
            public string Icon { get; set; } = string.Empty;
            public BaseViewModel ViewModel { get; set; } = null!;
            public bool IsSelected { get; set; }
        }
    }
}