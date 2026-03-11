using System.Collections.Generic;
using System.Collections.ObjectModel;
using App.DataTypes;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;

namespace App.ViewModels;

public partial class ShareItemViewModel(List<Seller> sellers, ObservableCollection<ShareItemViewModel> shareViewModels) : ObservableObject
{
    /// Raised when the user requests deletion of this share row.
    public event Action Deleted;

    /// Raised when the percentage value changes.
    public event Action PercentageChanged;

    public List<Seller> Sellers { get; } = sellers;

    [ObservableProperty]
    public partial Seller SelectedSeller { get; set; }

    [ObservableProperty]
    public partial int Percentage { get; set; }

    partial void OnPercentageChanged(int value) => PercentageChanged?.Invoke();

    public void Delete() => Deleted?.Invoke();

    /// Disables already-selected sellers in the dropdown when it opens.
    public void OnDropDownOpened(object sender, object _)
    {
        var comboBox = sender as ComboBox;
        // Defer until ComboBoxItem containers are materialized after the dropdown layout pass
        comboBox.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, () =>
        {
            var selectedSellerIds = shareViewModels
                .Where(viewModel => viewModel != this && viewModel.SelectedSeller != null)
                .Select(viewModel => viewModel.SelectedSeller.Id)
                .ToHashSet();

            foreach (var seller in Sellers)
            {
                if (comboBox.ContainerFromItem(seller) is ComboBoxItem container)
                    container.IsEnabled = !selectedSellerIds.Contains(seller.Id);
            }
        });
    }
}
