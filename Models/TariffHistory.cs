using System;
using System.Collections.Generic;
using System.Text;

namespace ElectricityApp.Models
{
    public class TariffHistory
    {
        public int Id { get; set; }
        public decimal Rate { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public string ChangedBy { get; set; }

        public override string ToString()
        {
            return $"{Rate} руб./кВт·ч (с {ValidFrom:dd.MM.yyyy})";
        }
    }
}