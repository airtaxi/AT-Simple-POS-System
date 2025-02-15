using App.DataTypes;

namespace App.ViewModels;

public partial class RecordViewModel
{
	public string RecordId { get; }
	public event EventHandler Deleted;

	public string TimestampText { get; }
	public string PriceText { get; }
	public string QuantityText { get; }

	public DateTime Timestamp { get; }

    public RecordViewModel(Record record)
	{
		RecordId = record.Id;

        TimestampText = record.Timestamp.ToLocalTime().ToString("yyyy/MM/dd HH:mm:ss.fff");
        PriceText = record.TotalPrice.ToString("N0");
		QuantityText = record.TotalQuantity.ToString("N0");

		Timestamp = record.Timestamp;
	}

	public void OnDeleteButtonClicked(object _, RoutedEventArgs __) => Deleted?.Invoke(this, EventArgs.Empty);

	public void OnUnloaded(object _, RoutedEventArgs __) => Deleted = null;

}
