using Microsoft.EntityFrameworkCore;
using МаршрутСборки.Data;
using МаршрутСборки.Models;

namespace МаршрутСборки.Services
{
    public class WarehouseService
    {
        private readonly AppDbContext _context;

        public WarehouseService(AppDbContext context)
        {
            _context = context;
        }

        public List<WarehouseOperation> GetHistory()
        {
            return _context.WarehouseOperations
                .Include(o => o.Component)
                .Include(o => o.User)
                .Include(o => o.Assembly)
                .OrderByDescending(o => o.OperationDate)
                .ToList();
        }

        public List<WarehouseOperation> GetHistoryFiltered(DateTime fromUtc, DateTime toUtc)
        {
            return _context.WarehouseOperations
                .Include(o => o.Component)
                .Include(o => o.User)
                .Include(o => o.Assembly)
                .Where(o => o.OperationDate >= fromUtc && o.OperationDate <= toUtc)
                .OrderByDescending(o => o.OperationDate)
                .ToList();
        }

        public void IssueComponents(int assemblyId, int userId, string documentRef)
        {
            var components = _context.AssemblyComponents
                .Include(ac => ac.Component)
                .Where(ac => ac.AssemblyId == assemblyId)
                .ToList();

            foreach (var ac in components)
            {
                if (ac.Component.StockBalance < ac.Quantity)
                    throw new InvalidOperationException(
                        $"Недостаточно запасов: «{ac.Component.Name}».\n" +
                        $"На складе: {ac.Component.StockBalance} шт., требуется: {ac.Quantity} шт.");

                ac.Component.StockBalance -= ac.Quantity;

                _context.WarehouseOperations.Add(new WarehouseOperation
                {
                    OperationType = OperationType.Issue,
                    ComponentId = ac.ComponentId,
                    AssemblyId = assemblyId,
                    UserId = userId,
                    Quantity = ac.Quantity,
                    DocumentRef = documentRef,
                    OperationDate = DateTime.UtcNow
                });
            }

            _context.SaveChanges();

            var assembly = _context.Assemblies.Find(assemblyId);
            var eventLog = new EventLogService(_context);
            eventLog.Log(userId, "Выдача комплектующих",
                $"Выданы комплектующие для сборки {assembly?.AssemblyNumber}");
        }

        public List<Models.Assembly> GetReworkAssemblies()
        {
            var ids = _context.AssemblyReworkItems
                .Where(r => !r.IsIssued)
                .Select(r => r.AssemblyId)
                .Distinct()
                .ToList();

            return _context.Assemblies
                .Include(a => a.Assembler)
                .Include(a => a.Dispatcher)
                .Include(a => a.AssemblyComponents).ThenInclude(ac => ac.Component)
                .Where(a => ids.Contains(a.AssemblyId))
                .OrderByDescending(a => a.CreationDate)
                .ToList();
        }

        public List<AssemblyReworkItem> GetPendingReworkItems(int assemblyId)
        {
            return _context.AssemblyReworkItems
                .Include(r => r.OldComponent)
                .Include(r => r.NewComponent)
                .Where(r => r.AssemblyId == assemblyId && !r.IsIssued)
                .ToList();
        }

        public List<AssemblyReworkItem> IssueReworkComponents(int assemblyId, int userId)
        {
            var items = _context.AssemblyReworkItems
                .Include(r => r.NewComponent)
                .Where(r => r.AssemblyId == assemblyId && !r.IsIssued)
                .ToList();

            var assembly = _context.Assemblies
                .Include(a => a.AssemblyComponents)
                .FirstOrDefault(a => a.AssemblyId == assemblyId);

            if (assembly == null) return new List<AssemblyReworkItem>();

            foreach (var item in items)
            {
                if (item.NewComponent.StockBalance < item.Quantity)
                    throw new InvalidOperationException(
                        $"Недостаточно запасов: «{item.NewComponent.Name}».\n" +
                        $"На складе: {item.NewComponent.StockBalance} шт., требуется: {item.Quantity} шт.");

                item.NewComponent.StockBalance -= item.Quantity;
                _context.WarehouseOperations.Add(new WarehouseOperation
                {
                    OperationType = OperationType.Issue,
                    ComponentId = item.NewComponentId,
                    AssemblyId = assemblyId,
                    UserId = userId,
                    Quantity = item.Quantity,
                    DocumentRef = $"Переделка {assembly.AssemblyNumber}",
                    Notes = item.Notes,
                    OperationDate = DateTime.UtcNow
                });

                if (item.OldComponentId.HasValue)
                {
                    var oldComp = _context.Components.Find(item.OldComponentId.Value);
                    if (oldComp != null)
                    {
                        oldComp.StockBalance += item.Quantity;
                        _context.WarehouseOperations.Add(new WarehouseOperation
                        {
                            OperationType = OperationType.Return,
                            ComponentId = item.OldComponentId.Value,
                            AssemblyId = assemblyId,
                            UserId = userId,
                            Quantity = item.Quantity,
                            DocumentRef = $"Возврат при переделке {assembly.AssemblyNumber}",
                            OperationDate = DateTime.UtcNow
                        });
                    }

                    var oldAc = assembly.AssemblyComponents
                        .FirstOrDefault(ac => ac.ComponentId == item.OldComponentId.Value);
                    if (oldAc != null)
                        _context.AssemblyComponents.Remove(oldAc);
                }

                var existing = assembly.AssemblyComponents
                    .FirstOrDefault(ac => ac.ComponentId == item.NewComponentId);
                if (existing != null)
                    existing.Quantity += item.Quantity;
                else
                    _context.AssemblyComponents.Add(new AssemblyComponent
                    {
                        AssemblyId = assemblyId,
                        ComponentId = item.NewComponentId,
                        Quantity = item.Quantity
                    });

                item.IsIssued = true;
            }

            // После выдачи переводим сборку в «В сборке» — сборщик видит что нужно начать
            assembly.Status = AssemblyStatus.InProgress;

            _context.SaveChanges();

            var eventLog = new EventLogService(_context);
            eventLog.Log(userId, "Выдача по переделке",
                $"Выданы комплектующие для переделки сборки {assembly.AssemblyNumber}");

            return items;
        }

        public void ReceiveDelivery(int componentId, int quantity, int userId, string documentRef)
        {
            var component = _context.Components.Find(componentId);
            if (component == null) return;

            component.StockBalance += quantity;

            _context.WarehouseOperations.Add(new WarehouseOperation
            {
                OperationType = OperationType.Receipt,
                ComponentId = componentId,
                UserId = userId,
                Quantity = quantity,
                DocumentRef = documentRef,
                OperationDate = DateTime.UtcNow
            });

            _context.SaveChanges();
        }
    }
}
