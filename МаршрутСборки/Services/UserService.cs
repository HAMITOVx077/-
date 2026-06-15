using Microsoft.EntityFrameworkCore;
using МаршрутСборки.Data;
using МаршрутСборки.Models;

namespace МаршрутСборки.Services
{
    public class UserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public User? Authenticate(string login, string password)
        {
            var user = _context.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Login == login && u.IsActive);

            if (user == null) return null;
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;

            var eventLog = new EventLogService(_context);
            eventLog.Log(user.UserId, "Вход в систему",
                $"{user.FullName} вошёл в систему ({user.Role.RoleName})");

            return user;
        }

        public List<User> GetAll()
        {
            return _context.Users
                .Include(u => u.Role)
                .Where(u => u.IsActive)
                .OrderBy(u => u.LastName)
                .ToList();
        }

        public List<User> GetByRole(int roleId)
        {
            return _context.Users
                .Include(u => u.Role)
                .Where(u => u.RoleId == roleId && u.IsActive)
                .ToList();
        }

        public void Create(User user)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public void Update(User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public void Deactivate(int userId)
        {
            var user = _context.Users.Find(userId);
            if (user != null)
            {
                user.IsActive = false;
                _context.SaveChanges();
            }
        }

        public List<Role> GetAllRoles()
        {
            return _context.Roles.ToList();
        }
    }
}