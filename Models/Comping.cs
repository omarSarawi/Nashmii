using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace test2.Models
{
    public class Comping
    {
        public int Id { get; set; }

        [Required]
        public string? Title { get; set; }

        public string? ShortDesc { get; set; }

        public string? LongDesc { get; set; }


        [Display(Name = "goal")]
        [Range(1, int.MaxValue, ErrorMessage = "The target must be a positive number")]
        public int? Goal { get; set; }

        [Display(Name = "combined amount")]
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be 0 or greater")]
        public int? Amount { get; set; }

        [ForeignKey("Category")]
        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        public Category? Category { get; set; }

        [ForeignKey("User")]
        [Display(Name = "User")]
        public int? UserId { get; set; }

        public User? User { get; set; }
        [Required]
        [Display(Name = "Upload Image")]

        public string? ImgUrl { get; set; }
    }
}