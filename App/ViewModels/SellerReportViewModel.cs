using App.DataTypes;

namespace App.ViewModels;

public class SellerReportItemViewModel(string itemName, int percentage, int quantity, int shareAmount)
{
    public string ItemName { get; } = itemName;
    public int Percentage { get; } = percentage;
    public string PercentageText { get; } = percentage.ToString("N0") + "%";
    public int Quantity { get; } = quantity;
    public string QuantityText { get; } = quantity.ToString("N0");
    public int ShareAmount { get; } = shareAmount;
    public string ShareAmountText { get; } = shareAmount.ToString("N0");
}

public class SellerReportViewModel
{
    public string SellerId { get; }
    public string SellerName { get; }
    public List<SellerReportItemViewModel> Items { get; }
    public int TotalShareAmount { get; }
    public string TotalShareAmountText { get; }

    public SellerReportViewModel(string sellerId, string sellerName, List<Item> items, List<Transaction> transactions)
    {
        SellerId = sellerId;
        SellerName = sellerName;

        var itemViewModels = new List<SellerReportItemViewModel>();
        int totalShareAmount = 0;

        foreach (var item in items)
        {
            // Find the share for this seller
            var share = item.Shares?.FirstOrDefault(x => x.SellerId == sellerId);
            int percentage;

            if (sellerId == null)
            {
                // Unassigned: items with no shares at all get 100%
                if (item.Shares != null && item.Shares.Count > 0) continue;
                percentage = 100;
            }
            else
            {
                if (share == null) continue;
                percentage = share.Percentage;
            }

            var salesQuantity = transactions
                .Where(x => x.ItemId == item.Id)
                .Sum(x => x.Quantity);

            if (salesQuantity == 0) continue;

            int shareAmount = (int)(item.Price * salesQuantity * percentage / 100.0);
            totalShareAmount += shareAmount;

            itemViewModels.Add(new SellerReportItemViewModel(item.Name, percentage, salesQuantity, shareAmount));
        }

        Items = itemViewModels;
        TotalShareAmount = totalShareAmount;
        TotalShareAmountText = totalShareAmount.ToString("N0");
    }

    public static List<SellerReportViewModel> BuildAll()
    {
        var sellers = SellerManager.GetSellers();
        var items = ItemManager.GetItems();
        var transactions = TransactionManager.GetTransactions();
        var result = new List<SellerReportViewModel>();

        // Per-seller reports
        foreach (var seller in sellers)
            result.Add(new SellerReportViewModel(seller.Id, seller.Name, items, transactions));

        // Unassigned report (items with no shares)
        var hasUnassigned = items.Any(item => item.Shares == null || item.Shares.Count == 0);
        if (hasUnassigned)
        {
            var unassignedName = Localization.GetLocalizedString("/SellersReportPage/UnassignedSellerName");
            result.Add(new SellerReportViewModel(null, unassignedName, items, transactions));
        }

        return result;
    }
}
