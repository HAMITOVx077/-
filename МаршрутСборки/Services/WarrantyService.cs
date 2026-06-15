using Microsoft.EntityFrameworkCore;
using МаршрутСборки.Data;
using МаршрутСборки.Models;

namespace МаршрутСборки.Services
{
    public class WarrantyService
    {
        private readonly AppDbContext _context;

        public WarrantyService(AppDbContext context)
        {
            _context = context;
        }

        public List<WarrantyCase> GetAll()
        {
            return _context.WarrantyCases
                .Include(w => w.Engineer)
                .Include(w => w.Assembly)
                    .ThenInclude(a => a!.Assembler)
                .Include(w => w.Assembly)
                    .ThenInclude(a => a!.AssemblyComponents)
                        .ThenInclude(ac => ac.Component)
                .Include(w => w.Assembly)
                    .ThenInclude(a => a!.Tests)
                        .ThenInclude(t => t.Tester)
                .OrderByDescending(w => w.ReceivedDate)
                .ToList();
        }

        public void Create(WarrantyCase warrantyCase)
        {
            var count = _context.WarrantyCases.Count() + 1;
            warrantyCase.CaseNumber = $"ГАР-{DateTime.Now.Year}-{count:D4}";
            warrantyCase.ReceivedDate = DateTime.UtcNow;
            warrantyCase.Status = WarrantyStatus.Received;

            _context.WarrantyCases.Add(warrantyCase);
            _context.SaveChanges();
        }

        public void UpdateStatus(int caseId, string newStatus, string? repairNotes = null)
        {
            var warrantyCase = _context.WarrantyCases.Find(caseId);
            if (warrantyCase != null)
            {
                warrantyCase.Status = newStatus;
                if (repairNotes != null)
                    warrantyCase.RepairNotes = repairNotes;
                if (newStatus == WarrantyStatus.Closed)
                    warrantyCase.ClosedDate = DateTime.UtcNow;

                _context.SaveChanges();
            }
        }
        public void Delete(int caseId)
        {
            var warrantyCase = _context.WarrantyCases.Find(caseId);
            if (warrantyCase != null)
            {
                _context.WarrantyCases.Remove(warrantyCase);
                _context.SaveChanges();
            }
        }
    }
}