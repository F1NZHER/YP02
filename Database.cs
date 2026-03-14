using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using ElectricityApp.Models;

namespace ElectricityApp
{
    public class Database
    {
        private string connectionString = "Data Source=electricity.db";

        public Database()
        {
            CreateTables();
        }

        private void CreateTables()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // Таблица абонентов - только Id, LastName, Address
                var sqlAbonents = @"
                    CREATE TABLE IF NOT EXISTS Abonents (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        LastName TEXT NOT NULL,
                        Address TEXT NOT NULL
                    )";

                using (var command = new SqliteCommand(sqlAbonents, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Таблица платежей
                var sqlPayments = @"
                    CREATE TABLE IF NOT EXISTS Payments (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        AbonentId INTEGER NOT NULL,
                        PaymentDate TEXT NOT NULL,
                        PrevValue REAL NOT NULL,
                        CurrentValue REAL NOT NULL,
                        Amount REAL NOT NULL,
                        FOREIGN KEY (AbonentId) REFERENCES Abonents(Id) ON DELETE CASCADE
                    )";

                using (var command = new SqliteCommand(sqlPayments, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        // ========== РАБОТА С АБОНЕНТАМИ ==========

        public void AddAbonent(Abonent abonent)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // Вставляем только LastName и Address
                var sql = "INSERT INTO Abonents (LastName, Address) VALUES (@lastName, @address)";

                using (var command = new SqliteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@lastName", abonent.LastName);
                    command.Parameters.AddWithValue("@address", abonent.Address);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<Abonent> GetAllAbonents()
        {
            var abonents = new List<Abonent>();

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                // Получаем всех абонентов
                var sql = "SELECT * FROM Abonents ORDER BY LastName";

                using (var command = new SqliteCommand(sql, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        abonents.Add(new Abonent
                        {
                            Id = reader.GetInt32(0),
                            LastName = reader.GetString(1),
                            Address = reader.GetString(2),
                            Payments = new List<Payment>()
                        });
                    }
                }

                // Для каждого абонента загружаем платежи
                foreach (var abonent in abonents)
                {
                    var sqlPayments = "SELECT * FROM Payments WHERE AbonentId = @id ORDER BY PaymentDate DESC";

                    using (var command = new SqliteCommand(sqlPayments, connection))
                    {
                        command.Parameters.AddWithValue("@id", abonent.Id);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                abonent.Payments.Add(new Payment
                                {
                                    Id = reader.GetInt32(0),
                                    AbonentId = reader.GetInt32(1),
                                    PaymentDate = DateTime.Parse(reader.GetString(2)),
                                    PrevValue = reader.GetDouble(3),
                                    CurrentValue = reader.GetDouble(4),
                                    Amount = reader.GetDouble(5)
                                });
                            }
                        }
                    }
                }
            }

            return abonents;
        }

        // ========== РАБОТА С ПЛАТЕЖАМИ ==========

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

        // ========== СПИСОК ДОЛЖНИКОВ ==========

        public List<Abonent> GetDebtors()
        {
            var allAbonents = GetAllAbonents();
            var debtors = new List<Abonent>();

            foreach (var abonent in allAbonents)
            {
                if (abonent.HasDebt)
                {
                    debtors.Add(abonent);
                }
            }

            return debtors;
        }

        // ========== ТЕСТОВЫЕ ДАННЫЕ ==========

        public void AddTestData()
        {
            try
            {
                // Очищаем таблицы (сначала платежи, потом абонентов)
                using (var connection = new SqliteConnection(connectionString))
                {
                    connection.Open();

                    using (var command = new SqliteCommand("DELETE FROM Payments", connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (var command = new SqliteCommand("DELETE FROM Abonents", connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    // Сбрасываем автоинкремент
                    using (var command = new SqliteCommand("DELETE FROM sqlite_sequence", connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                // Добавляем тестовых абонентов (ТОЛЬКО ИМЯ И АДРЕС)
                var testAbonents = new List<Abonent>
                {
                    new Abonent { LastName = "Иванов Иван Иванович", Address = "ул. Ленина, д. 1, кв. 5" },
                    new Abonent { LastName = "Петров Петр Петрович", Address = "ул. Гагарина, д. 10, кв. 42" },
                    new Abonent { LastName = "Сидорова Мария Сергеевна", Address = "пр. Мира, д. 15, кв. 8" },
                    new Abonent { LastName = "Кузнецов Андрей Николаевич", Address = "ул. Советская, д. 3, кв. 12" },
                    new Abonent { LastName = "Смирнова Елена Викторовна", Address = "ул. Пушкина, д. 7, кв. 24" }
                };

                foreach (var a in testAbonents)
                {
                    AddAbonent(a);
                }

                // Получаем добавленных абонентов с ID
                var abonents = GetAllAbonents();

                // Добавляем платежи
                foreach (var ab in abonents)
                {
                    if (ab.LastName.Contains("Иванов"))
                    {
                        // Платил 10 дней назад (нет долга)
                        AddPayment(new Payment
                        {
                            AbonentId = ab.Id,
                            PaymentDate = DateTime.Now.AddDays(-10),
                            PrevValue = 1000,
                            CurrentValue = 1250,
                            Amount = 250 * 5.0
                        });

                        // Еще платеж 40 дней назад
                        AddPayment(new Payment
                        {
                            AbonentId = ab.Id,
                            PaymentDate = DateTime.Now.AddDays(-40),
                            PrevValue = 1250,
                            CurrentValue = 1500,
                            Amount = 250 * 5.0
                        });
                    }
                    else if (ab.LastName.Contains("Петров"))
                    {
                        // Не платил 45 дней (должник)
                        AddPayment(new Payment
                        {
                            AbonentId = ab.Id,
                            PaymentDate = DateTime.Now.AddDays(-45),
                            PrevValue = 1500,
                            CurrentValue = 1700,
                            Amount = 200 * 5.0
                        });
                    }
                    else if (ab.LastName.Contains("Сидорова"))
                    {
                        // Платила 5 дней назад (нет долга)
                        AddPayment(new Payment
                        {
                            AbonentId = ab.Id,
                            PaymentDate = DateTime.Now.AddDays(-5),
                            PrevValue = 800,
                            CurrentValue = 950,
                            Amount = 150 * 5.0
                        });

                        // И еще платеж 60 дней назад (должник)
                        AddPayment(new Payment
                        {
                            AbonentId = ab.Id,
                            PaymentDate = DateTime.Now.AddDays(-60),
                            PrevValue = 2000,
                            CurrentValue = 2300,
                            Amount = 300 * 5.0
                        });
                    }
                    else if (ab.LastName.Contains("Кузнецов"))
                    {
                        // Не платил 60 дней (должник)
                        AddPayment(new Payment
                        {
                            AbonentId = ab.Id,
                            PaymentDate = DateTime.Now.AddDays(-60),
                            PrevValue = 2000,
                            CurrentValue = 2300,
                            Amount = 300 * 5.0
                        });
                    }
                    else if (ab.LastName.Contains("Смирнова"))
                    {
                        // Платила вчера (нет долга)
                        AddPayment(new Payment
                        {
                            AbonentId = ab.Id,
                            PaymentDate = DateTime.Now.AddDays(-1),
                            PrevValue = 3000,
                            CurrentValue = 3200,
                            Amount = 200 * 5.0
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при добавлении тестовых данных: {ex.Message}");
                throw;
            }
        }
    }
}