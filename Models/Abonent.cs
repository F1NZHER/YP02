namespace ElectricityApp.Models
{
    public class Abonent
    {
        public int Id { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public DateTime LastPaymentDate { get; set; }
        public bool HasDebt => LastPaymentDate == DateTime.MinValue ||
                              (DateTime.Today - LastPaymentDate).Days > 30;
        public int LocalityId { get; set; }
        public Locality Locality { get; set; }
    }
}