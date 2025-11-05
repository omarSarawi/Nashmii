using test2.Models;

public class BloodCompingViewModel
{
    public int Id { get; set; }
    public string Type { get; set; }
    public int? Amount { get; set; }
    public int? Goal { get; set; }
    public Hospital Hospital { get; set; }
}