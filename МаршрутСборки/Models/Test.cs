using System.ComponentModel.DataAnnotations;

namespace МаршрутСборки.Models
{
    public class Test
    {
        [Key]
        public int TestId { get; set; }
        public string Result { get; set; } = string.Empty;
        public string? Defects { get; set; }
        public DateTime TestDate { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }

        public int AssemblyId { get; set; }
        public Assembly Assembly { get; set; } = null!;

        public int TesterId { get; set; }
        public User Tester { get; set; } = null!;
    }

    public static class TestResult
    {
        public const string Passed = "Пройдено";
        public const string Failed = "Не пройдено";
    }
}