using System.Linq;
using System.Windows;
using System.Windows.Controls;
using МаршрутСборки.Data;
using МаршрутСборки.Helpers;
using МаршрутСборки.Models;
using МаршрутСборки.Services;

namespace МаршрутСборки.Views.Dialogs
{
    public partial class NewWarrantyCaseDialog : Window
    {
        private readonly WarrantyService _warrantyService;
        private readonly AppDbContext _context;
        private Models.Assembly? _foundAssembly;

        public NewWarrantyCaseDialog()
        {
            InitializeComponent();
            _context = new AppDbContext();
            _warrantyService = new WarrantyService(_context);
        }

        private void AssemblyNumberBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _foundAssembly = null;
            AssemblyErrorText.Visibility = Visibility.Collapsed;

            var text = AssemblyNumberBox.Text.Trim();
            if (text.Length < 2)
            {
                AssemblyPopup.IsOpen = false;
                return;
            }

            var results = _context.Assemblies
                .Where(a => a.AssemblyNumber.Contains(text))
                .OrderBy(a => a.AssemblyNumber)
                .Take(10)
                .ToList();

            AssemblyResultsList.ItemsSource = results;
            AssemblyPopup.IsOpen = results.Any();
        }

        private void AssemblyResultsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AssemblyResultsList.SelectedItem is Models.Assembly asm)
            {
                _foundAssembly = asm;
                AssemblyNumberBox.Text = asm.AssemblyNumber;
                ClientNameBox.Text = asm.ClientName;
                AssemblyPopup.IsOpen = false;
                AssemblyErrorText.Visibility = Visibility.Collapsed;
                ProblemBox.Focus();
            }
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ClientNameBox.Text) ||
                string.IsNullOrWhiteSpace(ProblemBox.Text))
            {
                MessageBox.Show("Заполните обязательные поля: имя клиента и описание проблемы.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int? assemblyId = null;
            if (!string.IsNullOrWhiteSpace(AssemblyNumberBox.Text))
            {
                // Try already-found via search, otherwise exact match
                if (_foundAssembly == null)
                {
                    _foundAssembly = _context.Assemblies
                        .FirstOrDefault(a => a.AssemblyNumber == AssemblyNumberBox.Text.Trim());
                }

                if (_foundAssembly == null)
                {
                    AssemblyErrorText.Visibility = Visibility.Visible;
                    AssemblyNumberBox.Focus();
                    return;
                }

                assemblyId = _foundAssembly.AssemblyId;
            }

            var warrantyCase = new WarrantyCase
            {
                ClientName      = ClientNameBox.Text.Trim(),
                ClientPhone     = ClientPhoneBox.Text.Trim(),
                ProblemDescription = ProblemBox.Text.Trim(),
                AssemblyId      = assemblyId,
                EngineerId      = SessionContext.CurrentUser!.UserId,
                Status          = WarrantyStatus.Received,
                ReceivedDate    = System.DateTime.UtcNow
            };

            _warrantyService.Create(warrantyCase);
            DialogResult = true;
        }
    }
}
