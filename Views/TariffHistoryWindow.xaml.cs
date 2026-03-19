using System.Windows;
using System.Collections.ObjectModel;
using ElectricityApp.Models;

namespace ElectricityApp
{
    public partial class TariffHistoryWindow : Window
    {
        private readonly Database _database;

        public TariffHistoryWindow(Database database)
        {
            InitializeComponent();
            _database = database;
            LoadTariffHistory();
        }

        private void LoadTariffHistory()
        {
            var history = _database.GetTariffHistory();
            dgTariffHistory.ItemsSource = history;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}