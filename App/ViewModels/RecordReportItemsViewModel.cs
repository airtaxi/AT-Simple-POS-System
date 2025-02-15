using App.DataTypes;

namespace App.ViewModels;

public class RecordReportItemsViewModel(Transaction transaction)
{
    private readonly Item _item = ItemManager.GetItem(transaction.ItemId);

    public string NameText => _item.Name;
    public string QuantityText => transaction.Quantity.ToString("N0");
}
