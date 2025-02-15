using App.DataTypes;

namespace App.ViewModels;

public partial class ItemReportViewModel(Item item)
{
    public string ItemId { get; } = item.Id;

    public string NameText { get; } = item.Name;
    public int Price { get; } = item.Price;
    public string PriceText { get; } = item.Price.ToString("N0");
    public int Quantity { get; } = item.SalesQuantity;
    public string QuantityText { get; } = item.SalesQuantity.ToString("N0");
    public int TotalPrice { get; } = item.Price * item.SalesQuantity;
    public string TotalPriceText { get; } = (item.Price * item.SalesQuantity).ToString("N0");
}
