using Microsoft.Data.Sqlite;
using ElectricityApp.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace ElectricityApp
{
    public class Database
    {
        private string connectionString = "Data Source=electricity.db";

        public void CreateTables()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var createAbonentsSql = @"
                    CREATE TABLE IF NOT EXISTS Abonents (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        LastName TEXT NOT NULL,
                        Address TEXT NOT NULL,
                        LocalityId INTEGER DEFAULT 1
                    )";

                using (var command = new SqliteCommand(createAbonentsSql, connection))
                {
                    command.ExecuteNonQuery();
                }

                var createPaymentsSql = @"
                    CREATE TABLE IF NOT EXISTS Payments (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        AbonentId INTEGER NOT NULL,
                        PaymentDate TEXT NOT NULL,
                        PrevValue REAL NOT NULL,
                        CurrentValue REAL NOT NULL,
                        Amount REAL NOT NULL,
                        FOREIGN KEY (AbonentId) REFERENCES Abonents(Id)
                    )";

                using (var command = new SqliteCommand(createPaymentsSql, connection))
                {
                    command.ExecuteNonQuery();
                }

                var createLocalitiesSql = @"
                    CREATE TABLE IF NOT EXISTS Localities (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Type INTEGER NOT NULL,
                        Region TEXT
                    )";

                using (var command = new SqliteCommand(createLocalitiesSql, connection))
                {
                    command.ExecuteNonQuery();
                }

                try
                {
                    var alterAbonentsSql = @"
                        ALTER TABLE Abonents ADD COLUMN LocalityId INTEGER DEFAULT 1";

                    using (var command = new SqliteCommand(alterAbonentsSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
                catch { }

                var insertLocalitiesSql = @"
                    INSERT OR IGNORE INTO Localities (Id, Name, Type, Region) VALUES
                    (1, 'Городской', 1, 'Область'),
                    (2, 'Сельский', 2, 'Область'),
                    (3, 'ПГТ', 3, 'Область'),
                    (4, 'С электроплитами', 4, 'Область')";

                using (var command = new SqliteCommand(insertLocalitiesSql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public void AddAbonent(Abonent abonent)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var sql = @"
                    INSERT INTO Abonents (LastName, Address, LocalityId)
                    VALUES (@lastName, @address, @localityId)";

                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@lastName", abonent.LastName);
                    command.Parameters.AddWithValue("@address", abonent.Address);
                    command.Parameters.AddWithValue("@localityId", abonent.LocalityId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateAbonent(Abonent abonent)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var sql = @"
                    UPDATE Abonents 
                    SET LastName = @lastName, 
                        Address = @address,
                        LocalityId = @localityId
                    WHERE Id = @id";

                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", abonent.Id);
                    command.Parameters.AddWithValue("@lastName", abonent.LastName);
                    command.Parameters.AddWithValue("@address", abonent.Address);
                    command.Parameters.AddWithValue("@localityId", abonent.LocalityId);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteAbonent(int abonentId)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var deletePaymentsSql = "DELETE FROM Payments WHERE AbonentId = @abonentId";
                using (var cmd = new SqliteCommand(deletePaymentsSql, connection))
                {
                    cmd.Parameters.AddWithValue("@abonentId", abonentId);
                    cmd.ExecuteNonQuery();
                }

                var deleteAbonentSql = "DELETE FROM Abonents WHERE Id = @id";
                using (var cmd = new SqliteCommand(deleteAbonentSql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", abonentId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public ObservableCollection<Abonent> GetAllAbonents()
        {
            var abonents = new ObservableCollection<Abonent>();
            var localities = GetAllLocalities();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var sql = "SELECT * FROM Abonents";

                using (var command = new SqliteCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var abonent = new Abonent
                        {
                            Id = reader.GetInt32(0),
                            LastName = reader.GetString(1),
                            Address = reader.GetString(2),
                            LocalityId = reader.IsDBNull(3) ? 1 : reader.GetInt32(3),
                            LastPaymentDate = GetLastPaymentDate(reader.GetInt32(0))
                        };

                        abonent.Locality = localities.FirstOrDefault(l => l.Id == abonent.LocalityId);
                        abonents.Add(abonent);
                    }
                }
            }

            return abonents;
        }

        public ObservableCollection<Abonent> SearchAbonents(string searchText)
        {
            var allAbonents = GetAllAbonents();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                return allAbonents;
            }

            var filtered = new ObservableCollection<Abonent>(
                allAbonents.Where(a =>
                    a.LastName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    a.Address.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0
                )
            );

            return filtered;
        }

        public ObservableCollection<Abonent> GetDebtors()
        {
            var allAbonents = GetAllAbonents();
            var debtors = new ObservableCollection<Abonent>();

            foreach (var abonent in allAbonents)
            {
                if (abonent.HasDebt)
                {
                    debtors.Add(abonent);
                }
            }

            return debtors;
        }

        private DateTime GetLastPaymentDate(int abonentId)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var sql = @"
                    SELECT PaymentDate FROM Payments 
                    WHERE AbonentId = @abonentId 
                    ORDER BY PaymentDate DESC 
                    LIMIT 1";

                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@abonentId", abonentId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return DateTime.Parse(reader.GetString(0));
                        }
                    }
                }
            }

            return DateTime.MinValue;
        }

        public void AddPayment(Payment payment)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var sql = @"
                    INSERT INTO Payments (AbonentId, PaymentDate, PrevValue, CurrentValue, Amount)
                    VALUES (@abonentId, @date, @prev, @current, @amount)";

                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@abonentId", payment.AbonentId);
                    command.Parameters.AddWithValue("@date", payment.PaymentDate.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@prev", payment.PrevValue);
                    command.Parameters.AddWithValue("@current", payment.CurrentValue);
                    command.Parameters.AddWithValue("@amount", payment.Amount);
                    command.ExecuteNonQuery();
                }
            }
        }

        public Payment GetLastPayment(int abonentId)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var sql = @"
                    SELECT * FROM Payments 
                    WHERE AbonentId = @abonentId 
                    ORDER BY PaymentDate DESC 
                    LIMIT 1";

                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@abonentId", abonentId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Payment
                            {
                                Id = reader.GetInt32(0),
                                AbonentId = reader.GetInt32(1),
                                PaymentDate = DateTime.Parse(reader.GetString(2)),
                                PrevValue = reader.GetDouble(3),
                                CurrentValue = reader.GetDouble(4),
                                Amount = reader.GetDouble(5)
                            };
                        }
                    }
                }
            }

            return null;
        }

        public ObservableCollection<Locality> GetAllLocalities()
        {
            var localities = new ObservableCollection<Locality>();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var sql = "SELECT * FROM Localities";

                using (var command = new SqliteCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        localities.Add(new Locality
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            Type = (LocalityType)reader.GetInt32(2),
                            Region = reader.IsDBNull(3) ? "" : reader.GetString(3)
                        });
                    }
                }
            }

            return localities;
        }

        public void AddLocality(Locality locality)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var sql = @"
                    INSERT INTO Localities (Name, Type, Region)
                    VALUES (@name, @type, @region)";

                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@name", locality.Name);
                    command.Parameters.AddWithValue("@type", (int)locality.Type);
                    command.Parameters.AddWithValue("@region", locality.Region ?? "");
                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateLocality(Locality locality)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var sql = @"
                    UPDATE Localities 
                    SET Name = @name, Type = @type, Region = @region
                    WHERE Id = @id";

                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", locality.Id);
                    command.Parameters.AddWithValue("@name", locality.Name);
                    command.Parameters.AddWithValue("@type", (int)locality.Type);
                    command.Parameters.AddWithValue("@region", locality.Region ?? "");
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DeleteLocality(int id)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var sql = "DELETE FROM Localities WHERE Id = @id";

                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.ExecuteNonQuery();
                }
            }
        }

        public double CalculateAmount(double prevValue, double currentValue, decimal tariff, LocalityType localityType)
        {
            if (currentValue < prevValue)
                throw new ArgumentException("Текущие показания не могут быть меньше предыдущих");

            var consumption = currentValue - prevValue;

            var coefficient = localityType switch
            {
                LocalityType.Urban => 1.0,
                LocalityType.Rural => 0.8,
                LocalityType.UrbanType => 0.9,
                LocalityType.ElectricStove => 0.7,
                _ => 1.0
            };

            return consumption * (double)tariff * coefficient;
        }

        public void AddTestData()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                var deletePayments = "DELETE FROM Payments";
                var deleteAbonents = "DELETE FROM Abonents";

                using (var cmd = new SqliteCommand(deletePayments, connection))
                    cmd.ExecuteNonQuery();

                using (var cmd = new SqliteCommand(deleteAbonents, connection))
                    cmd.ExecuteNonQuery();
            }

            AddAbonent(new Abonent { LastName = "Иванов", Address = "ул. Ленина, 1", LocalityId = 1 });
            AddAbonent(new Abonent { LastName = "Петров", Address = "ул. Мира, 15", LocalityId = 2 });
            AddAbonent(new Abonent { LastName = "Сидоров", Address = "пр. Победы, 30", LocalityId = 1 });
            AddAbonent(new Abonent { LastName = "Козлова", Address = "с. Новое, 5", LocalityId = 2 });
            AddAbonent(new Abonent { LastName = "Новиков", Address = "пгт. Центральный, 12", LocalityId = 3 });

            var abonents = GetAllAbonents();

            if (abonents.Count > 0)
            {
                AddPayment(new Payment
                {
                    AbonentId = abonents[0].Id,
                    PaymentDate = DateTime.Today.AddDays(-10),
                    PrevValue = 1200,
                    CurrentValue = 1350,
                    Amount = CalculateAmount(1200, 1350, 5.0m, LocalityType.Urban)
                });

                AddPayment(new Payment
                {
                    AbonentId = abonents[1].Id,
                    PaymentDate = DateTime.Today.AddDays(-45),
                    PrevValue = 800,
                    CurrentValue = 950,
                    Amount = CalculateAmount(800, 950, 5.0m, LocalityType.Rural)
                });

                AddPayment(new Payment
                {
                    AbonentId = abonents[2].Id,
                    PaymentDate = DateTime.Today.AddDays(-5),
                    PrevValue = 1500,
                    CurrentValue = 1680,
                    Amount = CalculateAmount(1500, 1680, 5.0m, LocalityType.Urban)
                });
            }
        }
        public ObservableCollection<TariffHistory> GetTariffHistory()
        {
            var history = new ObservableCollection<TariffHistory>();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var sql = "SELECT * FROM TariffHistory ORDER BY ValidFrom DESC";

                using (var command = new SqliteCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        history.Add(new TariffHistory
                        {
                            Id = reader.GetInt32(0),
                            Rate = reader.GetDecimal(1),
                            ValidFrom = DateTime.Parse(reader.GetString(2)),
                            ValidTo = reader.IsDBNull(3) ? null : DateTime.Parse(reader.GetString(3)),
                            ChangedBy = reader.IsDBNull(4) ? "" : reader.GetString(4)
                        });
                    }
                }
            }

            return history;
        }
    }
}