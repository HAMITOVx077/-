using System.Linq;
using System.Windows.Controls;
using МаршрутСборки.Models;
using МаршрутСборки.ViewModels;

namespace МаршрутСборки.Views
{
    public partial class AssembliesView : UserControl
    {
        public AssembliesView()
        {
            InitializeComponent();
        }

        private void AssembliesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is AssembliesViewModel vm && sender is DataGrid grid)
                vm.SelectedAssemblies = grid.SelectedItems.Cast<Assembly>().ToList();
        }
    }
}