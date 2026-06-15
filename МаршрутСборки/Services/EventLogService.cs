using Microsoft.EntityFrameworkCore;
using МаршрутСборки.Data;
using МаршрутСборки.Models;

namespace МаршрутСборки.Services
{
    public class EventLogService
    {
        private readonly AppDbContext _context;

        public EventLogService(AppDbContext context)
        {
            _context = context;
        }

        public void Log(int userId, string actionType, string description)
        {
            _context.EventLogs.Add(new EventLog
            {
                UserId = userId,
                ActionType = actionType,
                Description = description,
                ActionTime = DateTime.UtcNow
            });
            _context.SaveChanges();
        }

        public List<EventLog> GetAll()
        {
            return _context.EventLogs
                .Include(e => e.User)
                .OrderByDescending(e => e.ActionTime)
                .ToList();
        }
    }
}

