using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using ElectricityApp.Models;

namespace ElectricityApp
{
    public partial class AddPaymentWindow : Window
    {
        private readonly Database _database;
        private readonly Abonent _selectedAbonent;
        private readonly decimal _baseTariff = 5.0m;

        public AddPaymentWindow(Database database, Abonent abonent)
        {
            InitializeComponent();
            _database = database;
            _selectedAbonent = abonent;

            LoadAbonents();
            LoadLastPayment();
        }

        private void LoadAbonents()
        {
            var abonents = _database.GetAllAbonents();
            cmbAbonent.ItemsSource = abonents;

            if (_selectedAbonent != null)
            {
                cmbAbonent.SelectedValue = _selectedAbonent.Id;
            }
        }

        private void LoadLastPayment()
        {
            if (_selectedAbonent != null)
            {
                var lastPayment = _database.GetLastPayment(_selectedAbonent.Id);

                if (lastPayment != null)
                {
                    txtPrevValue.Text = lastPayment.CurrentValue.ToString();
                }
                else
                {
                    txtPrevValue.Text = "0";
                }

                var locality = _selectedAbonent.Locality;
                if (locality != null)
                {
                    txtTariff.Text = $"{_baseTariff} (×{locality.TariffCoefficient:F1} {locality.Name})";
                }
            }
            else
            {
                txtPrevValue.Text = "0";
                txtTariff.Text = _baseTariff.ToString();
            }
        }

        private double ParseDouble(string value)
        {
            if (double.TryParse(value, NumberStyles.Number,
                new CultureInfo("ru-RU"), out double result))
            {
                return result;
            }

            if (double.TryParse(value, NumberStyles.Number,
                new CultureInfo("en-US"), out result))
            {
                return result;
            }

            throw new FormatException("Неверный формат числа");
        }

        private void btnCalculate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbAbonent.SelectedItem == null)
                {
                    MessageBox.Show("Выберите абонента", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                double prevValue = ParseDouble(txtPrevValue.Text);
                double currentValue = ParseDouble(txtCurrentValue.Text);
                double tariff = ParseDouble(txtTariff.Text);

                var abonent = (Abonent)cmbAbonent.SelectedItem;
                decimal coefficient = abonent.Locality?.TariffCoefficient ?? 1.0m;

                double consumption = currentValue - prevValue;
                double amount = consumption * tariff * (double)coefficient;

                if (currentValue < prevValue)
                {
                    MessageBox.Show("Текущие показания не могут быть меньше предыдущих",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                tbResult.Text = $"Расход: {consumption:F1} кВт·ч\n" +
                               $"Тариф: {tariff} × {coefficient:F1} = {tariff * (double)coefficient:F2} руб./кВт·ч\n" +
                               $"Сумма к оплате: {amount:F2} руб.";
                btnSave.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbAbonent.SelectedItem is not Abonent selectedAbonent)
                {
                    MessageBox.Show("Выберите абонента", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                decimal coefficient = selectedAbonent.Locality?.TariffCoefficient ?? 1.0m;

                double prevValue = ParseDouble(txtPrevValue.Text);
                double currentValue = ParseDouble(txtCurrentValue.Text);
                double tariff = ParseDouble(txtTariff.Text);
                double consumption = currentValue - prevValue;
                double amount = consumption * tariff * (double)coefficient;

                var payment = new Payment
                {
                    AbonentId = selectedAbonent.Id,
                    PaymentDate = dpDate.SelectedDate ?? DateTime.Today,
                    PrevValue = prevValue,
                    CurrentValue = currentValue,
                    Amount = amount
                };

                _database.AddPayment(payment);
                MessageBox.Show($"Платёж успешно добавлен!\n" +
                               $"Тариф: {tariff} руб./кВт·ч\n" +
                               $"Коэффициент ({selectedAbonent.Locality?.Name}): ×{coefficient:F1}\n" +
                               $"Сумма: {amount:F2} руб.", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void cmbAbonent_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbAbonent.SelectedItem is Abonent selectedAbonent)
            {
                var lastPayment = _database.GetLastPayment(selectedAbonent.Id);

                if (lastPayment != null)
                {
                    txtPrevValue.Text = lastPayment.CurrentValue.ToString();
                }
                else
                {
                    txtPrevValue.Text = "0";
                }

                var locality = selectedAbonent.Locality;
                if (locality != null)
                {
                    txtTariff.Text = $"{_baseTariff} (×{locality.TariffCoefficient:F1} {locality.Name})";
                }
            }
        }
    }
}