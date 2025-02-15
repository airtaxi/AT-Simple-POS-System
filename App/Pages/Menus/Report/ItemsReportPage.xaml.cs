using System.Collections.ObjectModel;
using App.ViewModels;

namespace App.Pages.Menus.Report;

public sealed partial class ItemsReportPage : Page
{
    public ItemsReportPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        var viewModels = e.Parameter as ObservableCollection<ItemReportViewModel>;
        IvItems.ItemsSource = viewModels;

        TotalQuantityTextBlock.Text = viewModels.Sum(x => x.Quantity).ToString("N0");
        TotalPriceTextBlock.Text = viewModels.Sum(x => x.TotalPrice).ToString("N0");
    }
}
