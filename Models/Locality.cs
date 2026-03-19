namespace ElectricityApp.Models
{
    public enum LocalityType
    {
        Urban = 1,          // Город — тариф 100%
        Rural = 2,          // Село — тариф 80%
        UrbanType = 3,      // ПГТ — тариф 90%
        ElectricStove = 4   // С электроплитами — тариф 70%
    }

    public class Locality
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public LocalityType Type { get; set; }
        public string Region { get; set; }

        public decimal TariffCoefficient => Type switch
        {
            LocalityType.Urban => 1.0m,
            LocalityType.Rural => 0.8m,
            LocalityType.UrbanType => 0.9m,
            LocalityType.ElectricStove => 0.7m,
            _ => 1.0m
        };

        public override string ToString()
        {
            return Name;
        }
    }
}