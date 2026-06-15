using Microsoft.EntityFrameworkCore;
using МаршрутСборки.Data;
using МаршрутСборки.Helpers;
using МаршрутСборки.Models;

namespace МаршрутСборки.Services
{
    public class AssemblyService
    {
        private readonly AppDbContext _context;
        private readonly EventLogService _eventLog;

        public AssemblyService(AppDbContext context)
        {
            _context = context;
            _eventLog = new EventLogService(context);
        }

        public List<Assembly> GetAll()
        {
            return _context.Assemblies
                .Include(a => a.Dispatcher)
                .Include(a => a.Assembler)
                .Include(a => a.AssemblyComponents)
                    .ThenInclude(ac => ac.Component)
                .Include(a => a.Tests)
                    .ThenInclude(t => t.Tester)
                .OrderByDescending(a => a.CreationDate)
                .ToList();
        }

        public List<Assembly> GetByStatus(string status)
        {
            return _context.Assemblies
                .Include(a => a.Dispatcher)
                .Include(a => a.Assembler)
                .Include(a => a.AssemblyComponents)
                    .ThenInclude(ac => ac.Component)
                .Include(a => a.Tests)
                    .ThenInclude(t => t.Tester)
                .Where(a => a.Status == status)
                .OrderByDescending(a => a.CreationDate)
                .ToList();
        }

        public List<Assembly> GetByAssembler(int assemblerId)
        {
            return _context.Assemblies
                .Include(a => a.Dispatcher)
                .Include(a => a.AssemblyComponents)
                    .ThenInclude(ac => ac.Component)
                .Include(a => a.Tests)
                    .ThenInclude(t => t.Tester)
                .Where(a => a.AssemblerId == assemblerId)
                .OrderByDescending(a => a.CreationDate)
                .ToList();
        }


        public Models.Assembly? GetById(int id)
        {
            return _context.Assemblies
                .Include(a => a.Dispatcher)
                .Include(a => a.Assembler)
                .Include(a => a.AssemblyComponents)
                    .ThenInclude(ac => ac.Component)
                .Include(a => a.Tests)
                .FirstOrDefault(a => a.AssemblyId == id);
        }

        public void Create(Models.Assembly assembly, List<AssemblyComponent> components)
        {
            CreateBatch(assembly, components, 1);
        }

        public void CreateBatch(Models.Assembly template, List<AssemblyComponent> components, int quantity)
        {
            var baseCount = _context.Assemblies.Count() + 1;
            var year = DateTime.Now.Year;
            var baseNumber = $"СБ-{year}-{baseCount:D4}";

            for (var i = 1; i <= quantity; i++)
            {
                var assembly = new Models.Assembly
                {
                    AssemblyNumber = quantity > 1 ? $"{baseNumber}/{i}" : baseNumber,
                    ClientName = template.ClientName,
                    Notes = template.Notes,
                    Deadline = template.Deadline,
                    Priority = template.Priority,
                    DispatcherId = template.DispatcherId,
                    Status = AssemblyStatus.New,
                    CreationDate = DateTime.UtcNow
                };

                _context.Assemblies.Add(assembly);
                _context.SaveChanges();

                foreach (var comp in components)
                {
                    _context.AssemblyComponents.Add(new AssemblyComponent
                    {
                        AssemblyId = assembly.AssemblyId,
                        ComponentId = comp.ComponentId,
                        Quantity = comp.Quantity
                    });
                }
                _context.SaveChanges();
            }

            if (SessionContext.CurrentUser != null)
            {
                var msg = quantity > 1
                    ? $"Создана партия {baseNumber} ({quantity} шт.) для клиента {template.ClientName}"
                    : $"Создана сборка {baseNumber} для клиента {template.ClientName}";
                _eventLog.Log(SessionContext.CurrentUser.UserId, "Создание сборки", msg);
            }
        }

        public void Update(int assemblyId, string clientName, string? notes,
            DateTime? deadline, string priority, List<AssemblyComponent> components)
        {
            var assembly = _context.Assemblies
                .Include(a => a.AssemblyComponents)
                .FirstOrDefault(a => a.AssemblyId == assemblyId);
            if (assembly == null) return;

            assembly.ClientName = clientName;
            assembly.Notes = notes;
            assembly.Deadline = deadline;
            assembly.Priority = priority;

            _context.AssemblyComponents.RemoveRange(assembly.AssemblyComponents);

            foreach (var comp in components)
            {
                _context.AssemblyComponents.Add(new AssemblyComponent
                {
                    AssemblyId = assemblyId,
                    ComponentId = comp.ComponentId,
                    Quantity = comp.Quantity
                });
            }

            _context.SaveChanges();

            if (SessionContext.CurrentUser != null)
                _eventLog.Log(
                    SessionContext.CurrentUser.UserId,
                    "Редактирование сборки",
                    $"Сборка {assembly.AssemblyNumber} отредактирована");
        }

        public void UpdateStatus(int assemblyId, string newStatus)
        {
            var assembly = _context.Assemblies.Find(assemblyId);
            if (assembly != null)
            {
                var oldStatus = assembly.Status;
                assembly.Status = newStatus;
                _context.SaveChanges();

                if (SessionContext.CurrentUser != null)
                    _eventLog.Log(
                        SessionContext.CurrentUser.UserId,
                        "Смена статуса",
                        $"Сборка {assembly.AssemblyNumber}: {oldStatus} → {newStatus}");
            }
        }

        public void AssignAssembler(int assemblyId, int assemblerId)
        {
            var assembly = _context.Assemblies.Find(assemblyId);
            if (assembly != null)
            {
                assembly.AssemblerId = assemblerId;
                assembly.Status = AssemblyStatus.InProgress;
                _context.SaveChanges();

                if (SessionContext.CurrentUser != null)
                    _eventLog.Log(
                        SessionContext.CurrentUser.UserId,
                        "Принята в работу",
                        $"Сборка {assembly.AssemblyNumber} принята в работу");
            }
        }

        public void Delete(int assemblyId)
        {
            var assembly = _context.Assemblies
                .Include(a => a.AssemblyComponents)
                .Include(a => a.Tests)
                .Include(a => a.WarrantyCases)
                .Include(a => a.WarehouseOperations)
                .FirstOrDefault(a => a.AssemblyId == assemblyId);

            if (assembly == null) return;

            var assemblyNumber = assembly.AssemblyNumber;

            // Удаляем все связанные записи
            _context.WarehouseOperations.RemoveRange(assembly.WarehouseOperations);
            _context.Tests.RemoveRange(assembly.Tests);
            _context.AssemblyComponents.RemoveRange(assembly.AssemblyComponents);

            // Гарантийные случаи — убираем привязку к сборке вместо удаления
            foreach (var wc in assembly.WarrantyCases)
                wc.AssemblyId = null;

            _context.SaveChanges();

            if (SessionContext.CurrentUser != null)
                _eventLog.Log(
                    SessionContext.CurrentUser.UserId,
                    "Удаление сборки",
                    $"Удалена сборка {assemblyNumber}");

            _context.Assemblies.Remove(assembly);
            _context.SaveChanges();
        }
    }
}