using System.ComponentModel.DataAnnotations;

namespace МаршрутСборки.Models
{
    public class WarehouseOperation
    {
        [Key]
        public int OperationId { get; set; }
        public string OperationType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime OperationDate { get; set; } = DateTime.UtcNow;
        public string? DocumentRef { get; set; }
        public string? Notes { get; set; }

        public int ComponentId { get; set; }
        public Component Component { get; set; } = null!;

        public int? AssemblyId { get; set; }
        public Assembly? Assembly { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }

    public static class OperationType
    {
        public const string Receipt = "Поступление";
        public const string Issue = "Выдача";
        public const string Reserve = "Резерв";
        public const string Return = "Возврат";
    }
}