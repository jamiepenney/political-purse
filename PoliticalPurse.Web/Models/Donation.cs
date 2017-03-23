namespace PoliticalPurse.Web.Models
{
    public class Donation
    {
        public long Id { get; set; }
        public string Party { get; set; }
        public int Year { get; set; }
        public string Donee { get; set; }
        public int NumberOfDonations { get; set; }
        public int NumberOfDonees { get; set; }
        public string DoneeAddress { get; set; }
        public string Postcode { get; set; }
        public decimal Amount { get; set; }
    }
}