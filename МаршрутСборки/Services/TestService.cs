using Microsoft.EntityFrameworkCore;
using МаршрутСборки.Data;
using МаршрутСборки.Models;

namespace МаршрутСборки.Services
{
    public class TestService
    {
        private readonly AppDbContext _context;

        public TestService(AppDbContext context)
        {
            _context = context;
        }

        public List<Models.Assembly> GetPendingTests()
        {
            return _context.Assemblies
                .Include(a => a.Assembler)
                .Include(a => a.Dispatcher)
                .Where(a => a.Status == AssemblyStatus.ReadyForTest ||
                            a.Status == AssemblyStatus.OnTesting)
                .ToList();
        }

        public void SaveResult(Test test)
        {
            _context.Tests.Add(test);

            var assembly = _context.Assemblies.Find(test.AssemblyId);
            if (assembly != null)
            {
                assembly.Status = test.Result == TestResult.Passed
                    ? AssemblyStatus.Ready
                    : AssemblyStatus.Rework;
            }

            _context.SaveChanges();

            var eventLog = new EventLogService(_context);
            eventLog.Log(test.TesterId, "Результат тестирования",
                $"Сборка {assembly?.AssemblyNumber}: {test.Result}" +
                (string.IsNullOrEmpty(test.Defects) ? "" : $" — {test.Defects}"));
        }

        public List<Test> GetByAssembly(int assemblyId)
        {
            return _context.Tests
                .Include(t => t.Tester)
                .Where(t => t.AssemblyId == assemblyId)
                .OrderByDescending(t => t.TestDate)
                .ToList();
        }
    }
}