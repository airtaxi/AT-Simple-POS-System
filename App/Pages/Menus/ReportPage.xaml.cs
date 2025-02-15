using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using App.ViewModels;
using ClosedXML.Excel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace App.Pages.Menus;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ReportPage : Page
{
    private ObservableCollection<RecordViewModel> _recordViewModels = new();

    public ReportPage()
    {
        InitializeComponent();

        // Setup the view models
        var records = RecordManager.GetAllRecords();
        foreach (var record in records)
        {
            var viewModel = new RecordViewModel(record);
            viewModel.Deleted += OnRecordDeleted;
            _recordViewModels.Add(viewModel);
        }

        // Newer records first
        _recordViewModels = new(_recordViewModels.OrderByDescending(x => x.Timestamp));

        // Apply the view models
        IvRecords.ItemsSource = _recordViewModels;
    }

    private async void OnRecordDeleted(object sender, EventArgs e)
    {
        var viewModel = sender as RecordViewModel;

        // Ask the user if they want to delete the record
        var result = await this.ShowMessageDialogAsync(Constants.MessageDialogWarning, Localization.GetLocalizedString("/ReportPage/MessageDialogDeleteRecordConfirmationMessage").Replace("#P1", viewModel.TimestampText).Replace("#P2", viewModel.PriceText).Replace("#P3", viewModel.QuantityText), Constants.MessageDialogYes, Constants.MessageDialogNo);
        if (result != ContentDialogResult.Primary) return;

        // Remove the record
        var recordId = viewModel.RecordId;
        TransactionManager.RemoveTransactionsByRecordId(recordId);

        // Remove the view model
        viewModel.Deleted -= OnRecordDeleted;
        _recordViewModels.Remove(viewModel);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        var item = sender as FrameworkElement;
        var viewModel = item.DataContext as RecordViewModel;
        viewModel.OnUnloaded(sender, e);
    }

    private void OnDeleteButtonClicked(object sender, RoutedEventArgs e)
    {
        var item = sender as FrameworkElement;
        var viewModel = item.DataContext as RecordViewModel;
        viewModel.OnDeleteButtonClicked(sender, e);
    }

    private async void OnExportToExcelAppBarButtonClicked(object sender, RoutedEventArgs e)
    {
        // using ClosedXML
        // 아이템별 총 판매 내역을 엑셀로 내보내기 (각자가 아님)
        // 예: 아이템1, 15,000₩, 3개, 45,000₩

        // Create a new workbook
        using var workbook = new XLWorkbook();

        // Items Report
        {
            var worksheet = workbook.Worksheets.Add(Localization.GetLocalizedString("/ReportPage/ItemsReportWorksheet"));

            // Set the header
            worksheet.Cell("A1").Value = Localization.GetLocalizedString("/ReportPage/ItemsReportWorksheetItemNameCellText");
            worksheet.Cell("B1").Value = Localization.GetLocalizedString("/ReportPage/ItemsReportWorksheetItemPriceCellText");
            worksheet.Cell("C1").Value = Localization.GetLocalizedString("/ReportPage/ItemsReportWorksheetItemQuantityCellText");
            worksheet.Cell("D1").Value = Localization.GetLocalizedString("/ReportPage/ItemsReportWorksheetItemTotalPriceCellText");

            // Set the data
            var row = 2;
            var transactions = TransactionManager.GetTransactions();
            var items = ItemManager.GetItems();
            foreach (var item in items)
            {
                var itemTransactions = transactions.Where(x => x.ItemId == item.Id);
                var totalPrice = itemTransactions.Sum(x => x.Quantity * item.Price);
                var totalQuantity = itemTransactions.Sum(x => x.Quantity);
                worksheet.Cell($"A{row}").Value = item.Name;
                worksheet.Cell($"B{row}").Value = item.Price;
                worksheet.Cell($"C{row}").Value = totalQuantity;
                worksheet.Cell($"D{row}").Value = totalPrice;
                row++;
            }

            // All Item Total
            worksheet.Cell($"A{row}").Value = Localization.GetLocalizedString("/ReportPage/ItemsReportWorksheetTotalPriceCellText");
            worksheet.Cell($"C{row}").FormulaA1 = $"SUM(C2:C{row - 1})";
            worksheet.Cell($"D{row}").FormulaA1 = $"SUM(D2:D{row - 1})";
        }

        // Record Report
        {
            var worksheet = workbook.Worksheets.Add(Localization.GetLocalizedString("/ReportPage/RecordsReportWorksheet"));

            // Set the header
            worksheet.Cell("B1").Value = Localization.GetLocalizedString("/ReportPage/RecordsReportWorksheetTimestampCellText");
            worksheet.Cell("C1").Value = Localization.GetLocalizedString("/ReportPage/RecordsReportWorksheetPriceCellText");
            worksheet.Cell("D1").Value = Localization.GetLocalizedString("/ReportPage/RecordsReportWorksheetQuantityCellText");

            // Set the data
            var row = 2;
            foreach (var recordViewModel in _recordViewModels)
            {
                worksheet.Cell($"A{row}").Value = recordViewModel.TimestampText;
                worksheet.Cell($"B{row}").Value = recordViewModel.PriceText;
                worksheet.Cell($"C{row}").Value = recordViewModel.QuantityText;
                row++;
            }

            // All Record Total
            worksheet.Cell($"A{row}").Value = Localization.GetLocalizedString("/ReportPage/RecordsReportWorksheetTotalPriceCellText");
            worksheet.Cell($"B{row}").FormulaA1 = $"SUM(B2:B{row - 1})";
            worksheet.Cell($"C{row}").FormulaA1 = $"SUM(C2:C{row - 1})";
        }

        // Save the workbook
        var savePicker = new FileSavePicker();
#if WINDOWS

        // Get the current window's HWND by passing a Window object
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        // Associate the HWND with the file picker
        WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);
        savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
#endif
        savePicker.FileTypeChoices.Add(Localization.GetLocalizedString("/ReportPage/ReportXlsxFileTypeChoice"), [".xlsx"]);
        savePicker.SuggestedFileName = Localization.GetLocalizedString("/ReportPage/ReportSuggestedFileName");

        var file = await savePicker.PickSaveFileAsync();
        if (file == null) return;

        using (var stream = await file.OpenStreamForWriteAsync()) workbook.SaveAs(stream);

        // Show a message
        await this.ShowMessageDialogAsync(Constants.MessageDialogSuccess, Localization.GetLocalizedString("/ReportPage/MessageDialogExportToExcelSuccessMessage"), Constants.MessageDialogOk);
    }

    private async void OnClearAppBarButtonClicked(object sender, RoutedEventArgs e)
    {
        var result = await this.ShowMessageDialogAsync(Constants.MessageDialogWarning, Localization.GetLocalizedString("/ReportPage/MessageDialogClearRecordsConfirmationMessage"), Constants.MessageDialogYes, Constants.MessageDialogNo);
        if (result != ContentDialogResult.Primary) return;

        TransactionManager.ClearTransactions();
        _recordViewModels.Clear();
    }
}
