using System.ComponentModel.DataAnnotations;

namespace МаршрутСборки.Models
{
    public class WarrantyCase
    {
        [Key]
        public int CaseId { get; set; }
        public string CaseNumber { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public string ClientPhone { get; set; } = string.Empty;
        public string ProblemDescription { get; set; } = string.Empty;
        public string Status { get; set; } = WarrantyStatus.Received;
        public DateTime ReceivedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ClosedDate { get; set; }
        public string? RepairNotes { get; set; }

        public int? AssemblyId { get; set; }
        public Assembly? Assembly { get; set; }

        public int EngineerId { get; set; }
        public User Engineer { get; set; } = null!;
    }

    public static class WarrantyStatus
    {
        public const string Received = "Принято";
        public const string Diagnosing = "Диагностика";
        public const string InRepair = "В ремонте";
        public const string ReadyForPickup = "Готово к выдаче";
        public const string Closed = "Закрыто";
    }
}