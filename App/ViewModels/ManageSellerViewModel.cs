using App.DataTypes;
using CommunityToolkit.Mvvm.ComponentModel;

namespace App.ViewModels;

public partial class ManageSellerViewModel : ObservableObject
{
    private Seller seller;
    public Seller Seller
    {
        get => seller;
        set
        {
            seller = value;
            SetSeller(seller);
        }
    }

    public ManageSellerViewModel(Seller seller) => Seller = seller;

    public void SetSeller(Seller seller)
    {
        Name = seller.Name;
        Contact = seller.Contact ?? "";
    }

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial string Contact { get; set; }
}
