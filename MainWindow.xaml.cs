using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using ElectricityApp.Models;
using ElectricityApp.Services;

namespace ElectricityApp
{
    public partial class MainWindow : Window
    {
        private readonly Database _database;
        private readonly ExportService _exportService;

        public MainWindow()
        {
            InitializeComponent();
            _database = new Database();
            _exportService = new ExportService();
            _database.CreateTables();
        }

        private void LoadAbonents()
        {
            var abonents = _database.GetAllAbonents();
            dgAbonents.ItemsSource = abonents;
            tbCount.Text = $"Всего записей: {abonents.Count}";
            tbStatus.Text = "Данные загружены";
            txtSearch.Clear();
        }

        private void btnAddAbonent_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddAbonentWindow(_database);
            addWindow.Owner = this;

            if (addWindow.ShowDialog() == true)
            {
                LoadAbonents();
                tbStatus.Text = "Абонент добавлен";
            }
        }

        private void btnEditAbonent_Click(object sender, RoutedEventArgs e)
        {
            if (dgAbonents.SelectedItem is Abonent selectedAbonent)
            {
                var editWindow = new EditAbonentWindow(_database, selectedAbonent);
                editWindow.Owner = this;

                if (editWindow.ShowDialog() == true)
                {
                    LoadAbonents();
                    tbStatus.Text = "Абонент отредактирован";
                }
            }
            else
            {
                MessageBox.Show("Выберите абонента для редактирования", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnDeleteAbonent_Click(object sender, RoutedEventArgs e)
        {
            if (dgAbonents.SelectedItem is Abonent selectedAbonent)
            {
                var result = MessageBox.Show(
                    $"Вы действительно хотите удалить абонента \"{selectedAbonent.LastName}\"?\n\n" +
                    $"ВНИМАНИЕ: Вся история платежей также будет удалена!",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _database.DeleteAbonent(selectedAbonent.Id);
                        LoadAbonents();
                        MessageBox.Show($"Абонент \"{selectedAbonent.LastName}\" успешно удален",
                            "Успех",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        tbStatus.Text = "Абонент удален";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}",
                            "Ошибка",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите абонента для удаления", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnAddPayment_Click(object sender, RoutedEventArgs e)
        {
            if (dgAbonents.SelectedItem is Abonent selectedAbonent)
            {
                var addWindow = new AddPaymentWindow(_database, selectedAbonent);
                addWindow.Owner = this;

                if (addWindow.ShowDialog() == true)
                {
                    LoadAbonents();
                    tbStatus.Text = "Платёж добавлен";
                }
            }
            else
            {
                MessageBox.Show("Выберите абонента из списка", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnDebtors_Click(object sender, RoutedEventArgs e)
        {
            var debtorsWindow = new DebtorsWindow(_database);
            debtorsWindow.Owner = this;
            debtorsWindow.ShowDialog();
            tbStatus.Text = "Список должников открыт";
        }

        private void btnExportAll_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                FileName = $"Абоненты_{DateTime.Today:yyyyMMdd}.xlsx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    var abonents = _database.GetAllAbonents();
                    _exportService.ExportToExcel(abonents, saveDialog.FileName);
                    MessageBox.Show("Экспорт выполнен успешно!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    tbStatus.Text = "Данные экспортированы в Excel";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    tbStatus.Text = "Ошибка экспорта";
                }
            }
        }

        private void btnExportDebtors_Click(object sender, RoutedEventArgs e)
        {
            var saveDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                FileName = $"Должники_{DateTime.Today:yyyyMMdd}.xlsx"
            };

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    var debtors = _database.GetDebtors();
                    _exportService.ExportDebtorsToExcel(debtors, saveDialog.FileName);
                    MessageBox.Show("Экспорт должников выполнен успешно!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    tbStatus.Text = "Должники экспортированы в Excel";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    tbStatus.Text = "Ошибка экспорта";
                }
            }
        }

        private void btnLocality_Click(object sender, RoutedEventArgs e)
        {
            var window = new LocalityWindow(_database);
            window.Owner = this;
            window.ShowDialog();
            tbStatus.Text = "Управление местностями";
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadAbonents();
            tbStatus.Text = "Данные обновлены";
        }

        private void btnTestData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _database.AddTestData();
                LoadAbonents();
                MessageBox.Show("Тестовые данные загружены", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                tbStatus.Text = "Тестовые данные загружены";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                tbStatus.Text = "Ошибка загрузки тестовых данных";
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = txtSearch.Text.Trim();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                var abonents = _database.GetAllAbonents();
                dgAbonents.ItemsSource = abonents;
                tbCount.Text = $"Всего записей: {abonents.Count}";
            }
            else
            {
                var filteredAbonents = _database.SearchAbonents(searchText);
                dgAbonents.ItemsSource = filteredAbonents;
                tbCount.Text = $"Найдено: {filteredAbonents.Count}";
            }

            tbStatus.Text = string.IsNullOrWhiteSpace(searchText)
                ? "Показаны все абоненты"
                : $"Поиск: {searchText}";
        }

        private void btnClearSearch_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            LoadAbonents();
            tbStatus.Text = "Поиск очищен";
        }

        private void dgAbonents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgAbonents.SelectedItem != null)
            {
                btnAddPayment.IsEnabled = true;
                tbStatus.Text = "Абонент выбран";
            }
            else
            {
                btnAddPayment.IsEnabled = false;
                tbStatus.Text = "Выберите абонента";
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAbonents();
            btnAddPayment.IsEnabled = false;
            tbStatus.Text = "Приложение готово к работе";
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите выйти из приложения?",
                "Подтверждение выхода",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Учёт оплаты электроэнергии\n\n" +
                "Версия: 1.0\n" +
                "Разработано: 2026\n\n" +
                "Приложение для автоматизации учёта оплаты электроэнергии,\n" +
                "ведения базы абонентов и контроля задолженностей.",
                "О программе",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}