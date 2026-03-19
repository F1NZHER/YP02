using System;
using System.Windows;
using System.Windows.Controls;
using ElectricityApp.Models;

namespace ElectricityApp
{
    public partial class EditLocalityWindow : Window
    {
        private readonly Database _database;
        private readonly Locality _locality;
        private readonly bool _isEditMode;

        public EditLocalityWindow(Database database, Locality locality)
        {
            InitializeComponent();
            _database = database;
            _locality = locality;
            _isEditMode = locality != null;

            if (_isEditMode)
            {
                LoadData();
            }
            else
            {
                txtCoefficient.Text = "1.0";
            }
        }

        private void LoadData()
        {
            Title = "Редактирование типа местности";
            txtName.Text = _locality.Name;
            txtRegion.Text = _locality.Region;
            txtCoefficient.Text = _locality.TariffCoefficient.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название типа местности", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtCoefficient.Text, System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out decimal coefficient))
            {
                MessageBox.Show("Введите корректный коэффициент (например: 0.8, 1.0, 0.7)",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (coefficient <= 0 || coefficient > 2)
            {
                MessageBox.Show("Коэффициент должен быть в диапазоне от 0 до 2",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_isEditMode)
                {
                    _locality.Name = txtName.Text.Trim();
                    _locality.Region = txtRegion.Text.Trim();

                    _locality.Type = coefficient switch
                    {
                        >= 0.95m and <= 1.05m => LocalityType.Urban,
                        >= 0.75m and < 0.95m => LocalityType.UrbanType,
                        >= 0.6m and < 0.75m => LocalityType.ElectricStove,
                        _ => LocalityType.Rural
                    };

                    _database.UpdateLocality(_locality);

                    MessageBox.Show($"Тип местности \"{_locality.Name}\" успешно обновлен!\n" +
                                   $"Коэффициент: ×{coefficient:F1}",
                        "Успех",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    var type = coefficient switch
                    {
                        >= 0.95m and <= 1.05m => LocalityType.Urban,
                        >= 0.75m and < 0.95m => LocalityType.UrbanType,
                        >= 0.6m and < 0.75m => LocalityType.ElectricStove,
                        _ => LocalityType.Rural
                    };

                    var newLocality = new Locality
                    {
                        Name = txtName.Text.Trim(),
                        Type = type,
                        Region = txtRegion.Text.Trim()
                    };

                    _database.AddLocality(newLocality);

                    MessageBox.Show($"Тип местности \"{newLocality.Name}\" успешно добавлен!\n" +
                                   $"Коэффициент: ×{coefficient:F1}",
                        "Успех",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }

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
    }
}