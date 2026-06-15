using Microsoft.EntityFrameworkCore;
using МаршрутСборки.Data;
using МаршрутСборки.Models;

namespace МаршрутСборки.Services
{
    public class ComponentService
    {
        private readonly AppDbContext _context;

        public ComponentService(AppDbContext context)
        {
            _context = context;
        }

        public List<Component> GetAll()
        {
            return _context.Components
                .OrderBy(c => c.Category)
                .ThenBy(c => c.Name)
                .ToList();
        }

        public List<Component> GetLowStock()
        {
            return _context.Components
                .Where(c => c.StockBalance <= c.MinStock)
                .ToList();
        }

        public List<Component> Search(string query)
        {
            query = query.ToLower();
            return _context.Components
                .Where(c => c.Name.ToLower().Contains(query) ||
                            c.SKU.ToLower().Contains(query) ||
                            c.Category.ToLower().Contains(query))
                .ToList();
        }

        public void Create(Component component)
        {
            _context.Components.Add(component);
            _context.SaveChanges();
        }

        public void Update(Component component)
        {
            _context.Components.Update(component);
            _context.SaveChanges();
        }

        public void Delete(int componentId)
        {
            var component = _context.Components.Find(componentId);
            if (component != null)
            {
                _context.Components.Remove(component);
                _context.SaveChanges();
            }
        }
    }
}