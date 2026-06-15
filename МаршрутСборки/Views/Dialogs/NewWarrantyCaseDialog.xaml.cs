using System.Linq;
using System.Windows;
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

        public NewWarrantyCaseDialog()
        {
            InitializeComponent();
            _context = new AppDbContext();
            _warrantyService = new WarrantyService(_context);
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ClientNameBox.Text) ||
                string.IsNullOrWhiteSpace(ProblemBox.Text))
            {
                MessageBox.Show("Заполните обязательные поля",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int? assemblyId = null;
            if (!string.IsNullOrWhiteSpace(AssemblyNumberBox.Text))
            {
                var assembly = _context.Assemblies
                    .FirstOrDefault(a => a.AssemblyNumber == AssemblyNumberBox.Text.Trim());
                assemblyId = assembly?.AssemblyId;
            }

            var warrantyCase = new WarrantyCase
            {
                ClientName = ClientNameBox.Text.Trim(),
                ClientPhone = ClientPhoneBox.Text.Trim(),
                ProblemDescription = ProblemBox.Text.Trim(),
                AssemblyId = assemblyId,
                EngineerId = SessionContext.CurrentUser!.UserId,
                Status = WarrantyStatus.Received,
                ReceivedDate = System.DateTime.UtcNow
            };

            _warrantyService.Create(warrantyCase);
            DialogResult = true;
        }
    }
}