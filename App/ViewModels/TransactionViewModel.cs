using Microsoft.UI.Xaml.Controls;
using App.DataTypes;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Composition;
using Microsoft.UI.Windowing;

namespace App.ViewModels;

public partial class TransactionViewModel(Item item) : ObservableObject
{
	public event EventHandler QuantityChanged;
	public event EventHandler Deleted;

    public Item Item { get; init; } = item;

    public string ItemName => Item.Name;
    public string ItemPriceText => Item.Price.ToString("N0");
    public string TotalPriceText => (Item.Price * Quantity).ToString("N0");
    public string QuantityText => Quantity.ToString("N0");

    public void InvokeQuantityChangedEvent() => QuantityChanged?.Invoke(this, EventArgs.Empty);
    public async void OnQuantityNumberBoxValueChanged(NumberBox _, NumberBoxValueChangedEventArgs __)
    {
        await Task.Delay(100); // Uno bug workaround
        QuantityChanged?.Invoke(this, EventArgs.Empty);
    }

    public void OnDeleteButtonClicked(object _, RoutedEventArgs __) => Deleted?.Invoke(this, EventArgs.Empty);

	public void OnUnloaded(object _, RoutedEventArgs __)
	{
		QuantityChanged = null;
		Deleted = null;
	}

    public void IncreaseQuantity() => Quantity++;

    public void ToggleWideMode(bool isWide)
    {
        SpinButtonPlacementMode = isWide ? NumberBoxSpinButtonPlacementMode.Inline : NumberBoxSpinButtonPlacementMode.Hidden;
        QuantityColumnDefinition = isWide ? new(130, GridUnitType.Pixel) : new(60, GridUnitType.Pixel);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalPriceText))]
    [NotifyPropertyChangedFor(nameof(QuantityText))]
    public partial int Quantity { get; set; } = 1;

    [ObservableProperty]
    public partial NumberBoxSpinButtonPlacementMode SpinButtonPlacementMode { get; set; } = NumberBoxSpinButtonPlacementMode.Hidden;

    [ObservableProperty]
    public partial GridLength QuantityColumnDefinition { get; set; } = new(60, GridUnitType.Pixel);
}
