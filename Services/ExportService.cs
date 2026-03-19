using ClosedXML.Excel;
using System.Collections.ObjectModel;
using System.Drawing;
using ElectricityApp.Models;

namespace ElectricityApp.Services
{
    public class ExportService
    {
        public void ExportToExcel(ObservableCollection<Abonent> abonents, string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Абоненты");

                worksheet.Cell(1, 1).Value = "№";
                worksheet.Cell(1, 2).Value = "ФИО";
                worksheet.Cell(1, 3).Value = "Адрес";
                worksheet.Cell(1, 4).Value = "Местность";
                worksheet.Cell(1, 5).Value = "Последняя оплата";
                worksheet.Cell(1, 6).Value = "Должник";

                var headerRange = worksheet.Range(1, 1, 1, 6);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                int row = 2;
                foreach (var abonent in abonents)
                {
                    worksheet.Cell(row, 1).Value = abonent.Id;
                    worksheet.Cell(row, 2).Value = abonent.LastName;
                    worksheet.Cell(row, 3).Value = abonent.Address;
                    worksheet.Cell(row, 4).Value = abonent.Locality?.Name ?? "Не указано";
                    worksheet.Cell(row, 5).Value = abonent.LastPaymentDate != DateTime.MinValue
                        ? abonent.LastPaymentDate.ToString("dd.MM.yyyy")
                        : "Нет данных";
                    worksheet.Cell(row, 6).Value = abonent.HasDebt ? "Да" : "Нет";

                    if (abonent.HasDebt)
                    {
                        worksheet.Range(row, 1, row, 6).Style.Fill.BackgroundColor =
                            XLColor.FromColor(Color.FromArgb(100, 255, 150, 150));
                    }

                    row++;
                }

                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(filePath);
            }
        }

        public void ExportDebtorsToExcel(ObservableCollection<Abonent> debtors, string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Должники");

                worksheet.Cell(1, 1).Value = "№";
                worksheet.Cell(1, 2).Value = "ФИО";
                worksheet.Cell(1, 3).Value = "Адрес";
                worksheet.Cell(1, 4).Value = "Последняя оплата";
                worksheet.Cell(1, 5).Value = "Дней просрочки";

                var headerRange = worksheet.Range(1, 1, 1, 5);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.Red;
                headerRange.Style.Font.FontColor = XLColor.White;

                int row = 2;
                foreach (var abonent in debtors)
                {
                    var daysOverdue = abonent.LastPaymentDate != DateTime.MinValue
                        ? (DateTime.Today - abonent.LastPaymentDate).Days
                        : 999;

                    worksheet.Cell(row, 1).Value = abonent.Id;
                    worksheet.Cell(row, 2).Value = abonent.LastName;
                    worksheet.Cell(row, 3).Value = abonent.Address;
                    worksheet.Cell(row, 4).Value = abonent.LastPaymentDate != DateTime.MinValue
                        ? abonent.LastPaymentDate.ToString("dd.MM.yyyy")
                        : "Нет платежей";
                    worksheet.Cell(row, 5).Value = daysOverdue;

                    row++;
                }

                worksheet.Columns().AdjustToContents();
                workbook.SaveAs(filePath);
            }
        }
    }
}