using System;
using System.Collections.Generic;
using System.Linq;

namespace ElectricityApp.Models
{
    public class Abonent
    {
        public int Id { get; set; }                // номер абонента
        public string LastName { get; set; }        // фамилия
        public string Address { get; set; }         // адрес

        // Связь с платежами
        public List<Payment> Payments { get; set; } = new List<Payment>();

        // Дата последней оплаты
        public DateTime LastPaymentDate
        {
            get
            {
                if (Payments == null || Payments.Count == 0)
                    return DateTime.MinValue;
                return Payments.Max(p => p.PaymentDate);
            }
        }

        // Есть ли задолженность более месяца
        public bool HasDebt
        {
            get
            {
                if (LastPaymentDate == DateTime.MinValue)
                    return true;
                return (DateTime.Now - LastPaymentDate).Days > 30;
            }
        }
    }
}