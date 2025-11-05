using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace test2.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Hospital
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Hospital name is required.")]
        [StringLength(100, ErrorMessage = "Hospital name cannot be longer than 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }

        [Url(ErrorMessage = "Please enter a valid URL.")]
        public string? ImageURL { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Approver (admin) ID is required.")]
        public int? ApprovedByUserId { get; set; }

        public User? ApprovedByUser { get; set; }

        public int OwnerUserId { get; set; }
        public User? OwnerUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}
