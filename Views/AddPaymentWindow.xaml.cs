using System;
using System.Linq;
using System.Globalization;
using System.Windows;
using ElectricityApp.Models;

namespace ElectricityApp.Views
{
    public partial class AddPaymentWindow : Window
    {
        private Database db;
        private NumberFormatInfo numberFormat;

        public AddPaymentWindow(Database database)
        {
            InitializeComponent();
            db = database;

            // Настраиваем формат чисел для русской локализации
            numberFormat = new NumberFormatInfo();
            numberFormat.NumberDecimalSeparator = ",";
            numberFormat.NumberGroupSeparator = "";

            LoadAbonents();
        }

        private void LoadAbonents()
        {
            try
            {
                var abonents = db.GetAllAbonents();
                cmbAbonent.ItemsSource = abonents;
                if (abonents.Count > 0)
                    cmbAbonent.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки абонентов: {ex.Message}");
            }
        }

        private void cmbAbonent_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            try
            {
                if (cmbAbonent.SelectedItem is Abonent abonent)
                {
                    var lastPayment = abonent.Payments.OrderByDescending(p => p.PaymentDate).FirstOrDefault();
                    if (lastPayment != null)
                    {
                        txtPrevValue.Text = lastPayment.CurrentValue.ToString("0.##");
                    }
                    else
                    {
                        txtPrevValue.Text = "0";
                    }

                    txtCurrentValue.Clear();
                    tbResult.Text = "Введите текущие показания";
                    btnSave.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void btnCalculate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbAbonent.SelectedItem == null)
                {
                    MessageBox.Show("Выберите абонента!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверяем заполнение полей
                if (string.IsNullOrWhiteSpace(txtCurrentValue.Text))
                {
                    MessageBox.Show("Введите текущие показания!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Парсим числа с учетом русской локализации
                double prev = ParseDouble(txtPrevValue.Text);
                double current = ParseDouble(txtCurrentValue.Text);
                double tariff = ParseDouble(txtTariff.Text);

                if (current < prev)
                {
                    MessageBox.Show("Текущие показания не могут быть меньше предыдущих!",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                double consumption = current - prev;
                double amount = consumption * tariff;

                tbResult.Text = $"Сумма к оплате: {amount:F2} руб. (расход: {consumption:F2} кВт)";
                btnSave.IsEnabled = true;
            }
            catch (FormatException)
            {
                MessageBox.Show("Введите корректные числа!\n(используйте запятую для десятичных дробей)",
                              "Ошибка формата", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbAbonent.SelectedItem == null)
                {
                    MessageBox.Show("Выберите абонента!");
                    return;
                }

                var abonent = cmbAbonent.SelectedItem as Abonent;

                // Парсим числа
                double prev = ParseDouble(txtPrevValue.Text);
                double current = ParseDouble(txtCurrentValue.Text);
                double tariff = ParseDouble(txtTariff.Text);

                // Извлекаем сумму из текста результата
                string resultText = tbResult.Text;
                if (string.IsNullOrWhiteSpace(resultText) || resultText.Contains("Введите"))
                {
                    MessageBox.Show("Сначала выполните расчет!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Парсим сумму из результата
                double amount = 0;
                try
                {
                    string amountStr = resultText.Split(' ')[4].Replace("руб.", "").Replace("руб", "");
                    amount = ParseDouble(amountStr);
                }
                catch
                {
                    // Если не удалось распарсить, вычисляем заново
                    amount = (current - prev) * tariff;
                }

                var payment = new Payment
                {
                    AbonentId = abonent.Id,
                    PaymentDate = dpDate.SelectedDate ?? DateTime.Today,
                    PrevValue = prev,
                    CurrentValue = current,
                    Amount = amount
                };

                db.AddPayment(payment);

                MessageBox.Show("Платёж успешно сохранён!", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (FormatException)
            {
                MessageBox.Show("Введите корректные числа!", "Ошибка формата", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Метод для парсинга чисел с поддержкой разных форматов
        private double ParseDouble(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            // Заменяем точку на запятую для русской локализации
            value = value.Trim().Replace('.', ',');

            // Пробуем распарсить с учетом культуры
            if (double.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out double result))
                return result;

            // Если не получилось, пробуем с инвариантной культурой
            if (double.TryParse(value.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                return result;

            throw new FormatException($"Не удалось преобразовать '{value}' в число");
        }
    }
}