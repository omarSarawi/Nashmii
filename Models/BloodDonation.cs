using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace test2.Models
{
    public class BloodDonation
    {
        public int? Id { get; set; }

        [ForeignKey("User")]
        [Required]
        public int? UserId { get; set; }

        public User? User { get; set; }

        [ForeignKey("BloodComping")]
        [Required]
        public int? BloodCompingId { get; set; }

        public BloodComping? BloodComping { get; set; }
        public string? Status { get; set; } = "Pending";

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "قيمة التبرع يجب أن تكون أكبر من 0")]
        public int? Amount { get; set; }
    }
}
