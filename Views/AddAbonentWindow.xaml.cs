using System;
using System.Windows;
using ElectricityApp.Models;

namespace ElectricityApp.Views
{
    public partial class AddAbonentWindow : Window
    {
        private Database db;

        public AddAbonentWindow(Database database)
        {
            InitializeComponent();
            db = database;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            var abonent = new Abonent
            {
                LastName = txtLastName.Text.Trim(),
                Address = txtAddress.Text.Trim()
            };

            db.AddAbonent(abonent);

            MessageBox.Show("Абонент добавлен!");
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}