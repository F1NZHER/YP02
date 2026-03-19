using System.Windows;
using ElectricityApp.Models;

namespace ElectricityApp
{
    public partial class EditAbonentWindow : Window
    {
        private readonly Database _database;
        private readonly Abonent _abonent;

        public EditAbonentWindow(Database database, Abonent abonent)
        {
            InitializeComponent();
            _database = database;
            _abonent = abonent;

            LoadData();
            LoadLocalities();
        }

        private void LoadData()
        {
            txtFullName.Text = _abonent.LastName;
            txtAddress.Text = _abonent.Address;
        }

        private void LoadLocalities()
        {
            var localities = _database.GetAllLocalities();
            cmbLocality.ItemsSource = localities;
            cmbLocality.SelectedValue = _abonent.LocalityId;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text) ||
                string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                MessageBox.Show("Заполните все обязательные поля", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbLocality.SelectedValue == null)
            {
                MessageBox.Show("Выберите тип местности", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _abonent.LastName = txtFullName.Text.Trim();
                _abonent.Address = txtAddress.Text.Trim();
                _abonent.LocalityId = (int)cmbLocality.SelectedValue;

                _database.UpdateAbonent(_abonent);

                MessageBox.Show($"Данные абонента \"{_abonent.LastName}\" успешно обновлены!",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении данных: {ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}