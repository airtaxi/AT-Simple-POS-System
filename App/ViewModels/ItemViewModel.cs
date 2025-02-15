using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI;
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
        var quantityText = Localization.GetLocalizedString("/ItemsPage/ItemViewModelQuantityTemplateText");
        QuantityText = item.IsSoldout ? Localization.GetLocalizedString("/ItemsPage/ItemViewModelSoldOutText") : quantityText.Replace("#P1", item.SalesQuantity.ToString("N0")).Replace("#P2", item.StockQuantity.ToString("N0"));
        QuantityForeground = item.IsSoldout ? new SolidColorBrush(Colors.Red) : (SolidColorBrush)App.Instance.Resources.ThemeDictionaries["DefaultTextForegroundThemeBrush"];
    }

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial ImageSource Image { get; set; }

    [ObservableProperty]
    public partial string PriceText { get; set; }

    [ObservableProperty]
    public partial string QuantityText { get; set; }

    [ObservableProperty]
    public partial SolidColorBrush QuantityForeground { get; set; }
}
