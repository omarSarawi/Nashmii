using System;
using System.ComponentModel.DataAnnotations;
using test2.Models;

public class ContactRequest
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; }

    [Phone(ErrorMessage = "Please enter a valid phone number.")]
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters.")]
    public string Phone { get; set; }

    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
    public string Address { get; set; }

    [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters.")]
    public string Message { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    public ContactStatus Status { get; set; } = ContactStatus.Pending;

    [StringLength(100, ErrorMessage = "Category type cannot exceed 100 characters.")]
    public string CategoryType { get; set; }

    public string? ImageURL { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public int OwnerReqId { get; set; }
    public User? OwnerReqUser { get; set; }
}

public enum ContactStatus
{
    Pending,
    Approved,
    Rejected
}
