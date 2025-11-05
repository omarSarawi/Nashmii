using System.ComponentModel.DataAnnotations;

namespace test2.Models.ViewModels;

public class UserDonationViewModel
{
    public int CompingId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than zero")]
    public double Amount { get; set; }
}

