namespace test2.Models
{
    public class ProfileViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public int TotalDonationsCount { get; set; }
        public decimal TotalDonationAmount { get; set; }
        public int TotalUnitsDonated { get; set; }
        public DateTime? LastDonationDate { get; set; }
        public List<DonationRow> Donations { get; set; }
    }

    public class DonationRow
    {
        public int DonationId { get; set; }
        public string CampaignType { get; set; } = "";
        public DateTime Date { get; set; }           // add if you need sorting
        public decimal DonationAmount { get; set; }         // decimal is nicer for money
        public string UnitType { get; set; } = "";
    }
}