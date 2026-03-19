namespace ElectricityApp.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int AbonentId { get; set; }
        public DateTime PaymentDate { get; set; }
        public double PrevValue { get; set; }
        public double CurrentValue { get; set; }
        public double Amount { get; set; }

        public double Consumption => CurrentValue - PrevValue;
    }
}