using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.DataTypes;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media.Imaging;

namespace App.ViewModels;

public partial class ManageItemViewModel : ObservableObject
{
    private Item item;
    public Item Item
    {
        get => item;
        set
        {
            item = value;
            SetItem(item);
        }
    }

    public ManageItemViewModel(Item item) => Item = item;

    public void SetItem(Item item)
    {
        Name = item.Name;

        Price = item.Price;
        StockQuantity = item.StockQuantity;
        SalesQuantity = item.SalesQuantity;

        LoadImage();
    }

    public void LoadImage()
    {
        // Setup image if binary exists
        if (Item.ImageBinary != null) Image = Utils.GetBitmapImageFromByteArray(Item.ImageBinary);
        else Image = null; // Reset image if it binary is null
    }

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial BitmapImage Image { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PriceText))]
    public partial int Price { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StockQuantityText))]
    public partial int StockQuantity { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SalesQuantityText))]
    public partial int SalesQuantity { get; set; }

    public string PriceText => Price.ToString("N0");
    public string StockQuantityText => StockQuantity.ToString("N0");
    public string SalesQuantityText => SalesQuantity.ToString("N0");
}
