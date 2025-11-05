using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace test2.Models
{
    public class Donation
    {
        public int Id { get; set; }

        [Required]
        [ForeignKey("Comping")]
        public int? CompingId { get; set; }

        [Required]
        [ForeignKey("User")]
        public int? UserId { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public double Amount { get; set; }

        public Comping? Comping { get; set; }
        public User? User { get; set; }
    }
}
