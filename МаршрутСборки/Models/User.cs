using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace МаршрутСборки.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string LastName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;

        public ICollection<Assembly> AssignedAssemblies { get; set; } = new List<Assembly>();
        public ICollection<Assembly> CreatedAssemblies { get; set; } = new List<Assembly>();
        public ICollection<Test> Tests { get; set; } = new List<Test>();
        public ICollection<WarrantyCase> WarrantyCases { get; set; } = new List<WarrantyCase>();
        public ICollection<WarehouseOperation> WarehouseOperations { get; set; } = new List<WarehouseOperation>();
        public ICollection<EventLog> EventLogs { get; set; } = new List<EventLog>();

        public string FullName => $"{LastName} {FirstName}";
    }
}