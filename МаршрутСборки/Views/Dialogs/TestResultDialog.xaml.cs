using System.Windows;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using МаршрутСборки.Data;
using МаршрутСборки.Helpers;
using МаршрутСборки.Models;
using МаршрутСборки.Services;

namespace МаршрутСборки.Views.Dialogs
{
    public partial class TestResultDialog : Window
    {
        private readonly Models.Assembly _assembly;
        private readonly TestService _testService;
        private string _selectedResult = string.Empty;

        public TestResultDialog(Models.Assembly assembly)
        {
            InitializeComponent();
            _assembly = assembly;
            _testService = new TestService(new AppDbContext());
            AssemblyNumberText.Text = $"Сборка {assembly.AssemblyNumber} — {assembly.ClientName}";
        }

        private void Passed_Click(object sender, RoutedEventArgs e)
        {
            _selectedResult = TestResult.Passed;
            PassedBorder.BorderThickness = new Thickness(2);
            PassedBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(22, 163, 74));
            PassedBorder.Background = new SolidColorBrush(Color.FromRgb(240, 253, 244));
            FailedBorder.BorderThickness = new Thickness(1);
            FailedBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(254, 215, 170));
            FailedBorder.Background = new SolidColorBrush(Color.FromRgb(255, 247, 237));
        }

        private void Failed_Click(object sender, RoutedEventArgs e)
        {
            _selectedResult = TestResult.Failed;
            FailedBorder.BorderThickness = new Thickness(2);
            FailedBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(234, 88, 12));
            FailedBorder.Background = new SolidColorBrush(Color.FromRgb(255, 237, 213));
            PassedBorder.BorderThickness = new Thickness(1);
            PassedBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(134, 239, 172));
            PassedBorder.Background = new SolidColorBrush(Color.FromRgb(240, 253, 244));
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedResult))
            {
                MessageBox.Show("Выберите результат тестирования",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var test = new Test
            {
                AssemblyId = _assembly.AssemblyId,
                TesterId = SessionContext.CurrentUser!.UserId,
                Result = _selectedResult,
                Defects = DefectsBox.Text,
                Notes = NotesBox.Text,
                TestDate = DateTime.UtcNow
            };

            _testService.SaveResult(test);

            // Load full assembly with all relations needed for PDF + rework
            var context = new AppDbContext();
            var fullAssembly = context.Assemblies
                .Include(a => a.AssemblyComponents).ThenInclude(ac => ac.Component)
                .Include(a => a.Assembler)
                .Include(a => a.Dispatcher)
                .Include(a => a.Tests).ThenInclude(t => t.Tester)
                .FirstOrDefault(a => a.AssemblyId == _assembly.AssemblyId);

            if (fullAssembly != null)
            {
                // Если не пройдено — сначала настраиваем переделку
                if (_selectedResult == TestResult.Failed)
                    new ReworkComponentsDialog(fullAssembly).ShowDialog();

                // Затем печатаем акт (уже с учётом переделки)
                var lastTest = fullAssembly.Tests
                    .OrderByDescending(t => t.TestDate)
                    .FirstOrDefault();
                if (lastTest != null)
                {
                    var pdfService = new PdfService();
                    var pdf = pdfService.GenerateTestAct(lastTest, fullAssembly);
                    pdfService.SaveAndOpen(pdf,
                        $"Акт_тестирования_{fullAssembly.AssemblyNumber}.pdf");
                }
            }

            DialogResult = true;
        }
    }
}