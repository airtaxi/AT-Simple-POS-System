using App;
using App.DataTypes;

public class Item
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

	public string Name { get; set; }
    public int Price { get; set; }
    public int StockQuantity { get; set; }
    public int SalesQuantity {
        get
        {
            var transactions = TransactionManager.GetTransactions();
            return transactions.Where(x => x.ItemId == Id).Sum(x => x.Quantity);
        }
    }

    public bool IsManuallySoldout { get; set; }
    public bool IsSoldout => SalesQuantity >= StockQuantity || IsManuallySoldout;

    public byte[] ImageBinary { get; set; }

    public List<ItemSellerShare> Shares { get; set; } = [];
}
