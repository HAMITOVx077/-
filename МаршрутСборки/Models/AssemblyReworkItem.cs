using System.ComponentModel.DataAnnotations;

namespace МаршрутСборки.Models
{
    public class AssemblyReworkItem
    {
        [Key]
        public int ReworkItemId { get; set; }

        public int AssemblyId { get; set; }
        public Assembly Assembly { get; set; } = null!;

        public int? OldComponentId { get; set; }
        public Component? OldComponent { get; set; }

        public int NewComponentId { get; set; }
        public Component NewComponent { get; set; } = null!;

        public int Quantity { get; set; } = 1;
        public bool IsIssued { get; set; } = false;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
