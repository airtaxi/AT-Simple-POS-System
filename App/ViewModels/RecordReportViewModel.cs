using App.DataTypes;

namespace App.ViewModels;

public partial class RecordReportViewModel(Record record)
{
    public string RecordId { get; } = record.Id;
    public event EventHandler Deleted;

    public IEnumerable<RecordReportItemsViewModel> RecordReportItemsViewModels { get; } = record.Transactions.Select(x => new RecordReportItemsViewModel(x));

    public string TimestampText { get; } = record.Timestamp.ToLocalTime().ToString("yyyy/MM/dd HH:mm:ss");
    public int TotalPrice { get; } = record.TotalPrice;
    public int TotalQuantity { get; } = record.TotalQuantity;
    public string TotalPriceText { get; } = record.TotalPrice.ToString("N0");
    public string TotalQuantityText { get; } = record.TotalQuantity.ToString("N0");

    public DateTime Timestamp { get; } = record.Timestamp;

    public void OnDeleteButtonClicked(object _, RoutedEventArgs __) => Deleted?.Invoke(this, EventArgs.Empty);

	public void OnUnloaded(object _, RoutedEventArgs __) => Deleted = null;

}
