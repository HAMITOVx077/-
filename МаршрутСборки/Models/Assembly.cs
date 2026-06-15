using System.ComponentModel.DataAnnotations;
using static System.Net.Mime.MediaTypeNames;

namespace МаршрутСборки.Models
{
    public class Assembly
    {
        [Key]
        public int AssemblyId { get; set; }
        public string AssemblyNumber { get; set; } = string.Empty;
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
        public DateTime? Deadline { get; set; }
        public string Status { get; set; } = AssemblyStatus.New;
        public string ClientName { get; set; } = string.Empty;
        public string Priority { get; set; } = AssemblyPriority.Medium;
        public string? Notes { get; set; }
        public string? Configuration { get; set; }

        public int DispatcherId { get; set; }
        public User Dispatcher { get; set; } = null!;

        public int? AssemblerId { get; set; }
        public User? Assembler { get; set; }

        public ICollection<AssemblyComponent> AssemblyComponents { get; set; } = new List<AssemblyComponent>();
        public ICollection<Test> Tests { get; set; } = new List<Test>();
        public ICollection<WarrantyCase> WarrantyCases { get; set; } = new List<WarrantyCase>();
        public ICollection<WarehouseOperation> WarehouseOperations { get; set; } = new List<WarehouseOperation>();
    }

    public static class AssemblyStatus
    {
        public const string New = "Новая";
        public const string WaitingComponents = "Ожидает комплектации";
        public const string InProgress = "В сборке";
        public const string ReadyForTest = "Готова к тестированию";
        public const string OnTesting = "На тестировании";
        public const string Ready = "Готова";
        public const string Shipped = "Отгружена";
        public const string Rework = "Требует доработки";
    }

    public static class AssemblyPriority
    {
        public const string Low = "Низкий";
        public const string Medium = "Средний";
        public const string High = "Высокий";
        public const string Critical = "Критический";
    }

}