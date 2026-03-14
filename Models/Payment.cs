using System;

namespace ElectricityApp.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int AbonentId { get; set; }          // номер абонента
        public DateTime PaymentDate { get; set; }    // дата оплаты
        public double PrevValue { get; set; }        // предыдущие показания
        public double CurrentValue { get; set; }     // текущие показания
        public double Amount { get; set; }           // размер оплаты

        // Расход электроэнергии
        public double Consumption
        {
            get { return CurrentValue - PrevValue; }
        }
    }
}