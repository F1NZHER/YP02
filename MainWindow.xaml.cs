using System;
using System.Windows;
using ElectricityApp.Views;

namespace ElectricityApp
{
    public partial class MainWindow : Window
    {
        private Database db;

        public MainWindow()
        {
            InitializeComponent();
            db = new Database();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var list = db.GetAllAbonents();
                dgAbonents.ItemsSource = list;
                tbCount.Text = $"Всего абонентов: {list.Count}";
                tbStatus.Text = "Данные загружены";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private void btnAddAbonent_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddAbonentWindow(db);
            window.Owner = this;
            window.ShowDialog();
            LoadData();
        }

        private void btnAddPayment_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddPaymentWindow(db);
            window.Owner = this;
            window.ShowDialog();
            LoadData();
        }

        private void btnDebtors_Click(object sender, RoutedEventArgs e)
        {
            var debtors = db.GetDebtors();
            var window = new DebtorsWindow(debtors);
            window.Owner = this;
            window.ShowDialog();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void btnTestData_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Добавить тестовые данные?\n(Текущие данные будут удалены)",
                                        "Подтверждение",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    db.AddTestData();
                    LoadData();
                    MessageBox.Show("Тестовые данные добавлены!",
                                  "Готово",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}",
                                  "Ошибка",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                }
            }
        }
    }
}