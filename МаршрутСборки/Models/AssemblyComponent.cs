using System.ComponentModel.DataAnnotations;

namespace МаршрутСборки.Models
{
    public class AssemblyComponent
    {
        [Key]
        public int Id { get; set; }
        public int AssemblyId { get; set; }
        public Assembly Assembly { get; set; } = null!;

        public int ComponentId { get; set; }
        public Component Component { get; set; } = null!;

        public int Quantity { get; set; }
    }
}