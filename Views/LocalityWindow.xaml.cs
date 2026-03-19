using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using ElectricityApp.Models;

namespace ElectricityApp
{
    public partial class LocalityWindow : Window
    {
        private readonly Database _database;
        private ObservableCollection<Locality> _localities;

        public LocalityWindow(Database database)
        {
            InitializeComponent();
            _database = database;
            LoadLocalities();
        }

        private void LoadLocalities()
        {
            _localities = _database.GetAllLocalities();
            dgLocalities.ItemsSource = _localities;
        }

        private void dgLocalities_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgLocalities.SelectedItem != null)
            {
                btnEdit.IsEnabled = true;
                btnDelete.IsEnabled = true;
            }
            else
            {
                btnEdit.IsEnabled = false;
                btnDelete.IsEnabled = false;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new EditLocalityWindow(_database, null);
            editWindow.Owner = this;

            if (editWindow.ShowDialog() == true)
            {
                LoadLocalities();
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgLocalities.SelectedItem is Locality selectedLocality)
            {
                var editWindow = new EditLocalityWindow(_database, selectedLocality);
                editWindow.Owner = this;

                if (editWindow.ShowDialog() == true)
                {
                    LoadLocalities();
                }
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgLocalities.SelectedItem is Locality selectedLocality)
            {
                var abonents = _database.GetAllAbonents();
                var abonentsWithThisLocality = abonents.Where(a => a.LocalityId == selectedLocality.Id).ToList();

                if (abonentsWithThisLocality.Count > 0)
                {
                    MessageBox.Show(
                        $"Невозможно удалить тип местности \"{selectedLocality.Name}\".\n" +
                        $"С этим типом связано {abonentsWithThisLocality.Count} абонент(ов).\n\n" +
                        $"Сначала измените тип местности у этих абонентов.",
                        "Ошибка удаления",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Вы действительно хотите удалить тип местности \"{selectedLocality.Name}\"?\n\n" +
                    $"Название: {selectedLocality.Name}\n" +
                    $"Коэффициент: ×{selectedLocality.TariffCoefficient:F1}",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _database.DeleteLocality(selectedLocality.Id);
                        LoadLocalities();
                        MessageBox.Show($"Тип местности \"{selectedLocality.Name}\" успешно удален",
                            "Успех",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
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
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}