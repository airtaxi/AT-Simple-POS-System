using System.Collections.ObjectModel;
using App.ViewModels;

namespace App.Pages.Menus.Report;

public sealed partial class SellersReportPage : Page
{
    public SellersReportPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        var viewModels = e.Parameter as ObservableCollection<SellerReportViewModel>;
        IvSellers.ItemsSource = viewModels;

        TotalShareAmountTextBlock.Text = viewModels.Sum(x => x.TotalShareAmount).ToString("N0");
    }
}
