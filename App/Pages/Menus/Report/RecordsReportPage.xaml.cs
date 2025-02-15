using System.Collections.ObjectModel;
using App.ViewModels;

namespace App.Pages.Menus.Report;

public sealed partial class RecordsReportPage : Page
{
    public RecordsReportPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        var viewModels = e.Parameter as ObservableCollection<RecordReportViewModel>;
        IvRecords.ItemsSource = viewModels;

        TotalPriceTextBlock.Text = viewModels.Sum(x => x.TotalPrice).ToString("N0");
        TotalQuantityTextBlock.Text = viewModels.Sum(x => x.TotalQuantity).ToString("N0");
    }

    private void OnViewModelUnloaded(object sender, RoutedEventArgs e)
    {
        var item = sender as FrameworkElement;
        var viewModel = item.DataContext as RecordReportViewModel;
        viewModel.OnUnloaded(sender, e);
    }

    private void OnViewModelDeleteButtonClicked(object sender, RoutedEventArgs e)
    {
        var item = sender as FrameworkElement;
        var viewModel = item.DataContext as RecordReportViewModel;
        viewModel.OnDeleteButtonClicked(sender, e);
    }
}
