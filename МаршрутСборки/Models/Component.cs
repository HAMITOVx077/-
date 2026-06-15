using System.ComponentModel.DataAnnotations;

namespace МаршрутСборки.Models
{
    public class Component
    {
        [Key]
        public int ComponentId { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockBalance { get; set; }
        public int MinStock { get; set; } = 5;

        public ICollection<AssemblyComponent> AssemblyComponents { get; set; } = new List<AssemblyComponent>();
        public ICollection<WarehouseOperation> WarehouseOperations { get; set; } = new List<WarehouseOperation>();

        public bool IsLowStock => StockBalance <= MinStock;
    }
}