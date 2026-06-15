using System.Windows;
using МаршрутСборки.Models;
using МаршрутСборки.ViewModels;

namespace МаршрутСборки.Views.Dialogs
{
    public partial class NewAssemblyDialog : Window
    {
        public NewAssemblyDialog(Assembly? existing = null)
        {
            InitializeComponent();
            DataContext = new NewAssemblyViewModel(existing);
        }
    }
}