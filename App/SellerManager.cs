using App.DataTypes;

namespace App;

public static class SellerManager
{
    public static List<Seller> GetSellers() =>
        Configuration.GetValue<List<Seller>>("Sellers") ?? [];

    public static Seller GetSeller(string sellerId)
    {
        var sellers = Configuration.GetValue<List<Seller>>("Sellers");
        if (sellers == null) return null;
        return sellers.FirstOrDefault(x => x.Id == sellerId);
    }

    private static void AddSeller(Seller seller)
    {
        var sellers = GetSellers();
        sellers.Add(seller);
        Configuration.SetValue("Sellers", sellers);
        Configuration.WriteBuffer();
    }

    public static void SaveSeller(Seller seller)
    {
        var sellers = GetSellers();
        var index = sellers.FindIndex(x => x.Id == seller.Id);

        if (index < 0)
        {
            AddSeller(seller);
            return;
        }

        sellers[index] = seller;
        Configuration.SetValue("Sellers", sellers);
        Configuration.WriteBuffer();
    }

    public static void RemoveSeller(string sellerId)
    {
        var sellers = GetSellers();
        sellers.RemoveAll(x => x.Id == sellerId);
        Configuration.SetValue("Sellers", sellers);
        Configuration.WriteBuffer();

        // Remove the seller from all item shares
        var items = ItemManager.GetItems();
        foreach (var item in items)
        {
            var removed = item.Shares.RemoveAll(x => x.SellerId == sellerId);
            if (removed > 0) ItemManager.SaveItem(item);
        }
    }

    public static void ClearSellers()
    {
        Configuration.SetValue("Sellers", new List<Seller>());
        Configuration.WriteBuffer();
    }
}
