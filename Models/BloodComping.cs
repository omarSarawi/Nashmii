using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace test2.Models
{
    public class BloodComping
    {
        public int Id { get; set; }

        [Required]
        public string? Type { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "الكمية يجب أن تكون رقم موجب")]
        public int? Amount { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "الهدف يجب أن يكون أكبر من 0")]
        public int? Goal { get; set; }

        [ForeignKey("Hospital")]
        public int? HospitalId { get; set; }

        public Hospital? Hospital { get; set; }

        [ForeignKey("User")]
        public int? UserId { get; set; }

        public User? User { get; set; }
    }
}
