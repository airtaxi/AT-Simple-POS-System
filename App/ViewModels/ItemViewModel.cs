using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;

namespace App.ViewModels;

public partial class ItemViewModel : ObservableObject
{
    public string ItemId;

	public ItemViewModel(string itemId)
	{
		ItemId = itemId;
		Refresh();
	}

	public void Refresh()
	{
		var items = ItemManager.GetItems();

		//Find the item
		var item = items.FirstOrDefault(x => x.Id == ItemId);
		if (item == null) return;

		// Set the properties
		Name = item.Name;

        // Setup image if binary exists
        if (item.ImageBinary != null) Image = Utils.GetBitmapImageFromByteArray(item.ImageBinary);
        else Image = null; // Reset image if it binary is null

		PriceText = $"{item.Price:N0}";
		QuantityText = $"Sold {item.SalesQuantity} out of {item.StockQuantity}";
	}

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial ImageSource Image { get; set; }

    [ObservableProperty]
    public partial string PriceText { get; set; }

    [ObservableProperty]
    public partial string QuantityText { get; set; }
}
