using System.Collections.Generic;
using System.Windows;
using ElectricityApp.Models;

namespace ElectricityApp.Views
{
    public partial class DebtorsWindow : Window
    {
        public DebtorsWindow(List<Abonent> debtors)
        {
            InitializeComponent();
            dgDebtors.ItemsSource = debtors;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}