using System.Windows;

namespace ElectricityApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var db = new Database();
            db.CreateTables(); // Теперь включает CreateNotificationsTable()
        }
    }
}