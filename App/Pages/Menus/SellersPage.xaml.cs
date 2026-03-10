using System.Collections.ObjectModel;
using App.DataTypes;
using App.ViewModels;

namespace App.Pages.Menus;

public sealed partial class SellersPage : Page
{
    private readonly ObservableCollection<ManageSellerViewModel> _viewModels = new();
    private ManageSellerViewModel _currentSellerViewModel;

    public SellersPage()
    {
        InitializeComponent();

        var sellers = SellerManager.GetSellers();
        var viewModels = sellers.Select(seller => new ManageSellerViewModel(seller));
        foreach (var viewModel in viewModels) _viewModels.Add(viewModel);
        LvSellers.ItemsSource = _viewModels;

        UpdateEmptyState();
    }

    private void UpdateEmptyState()
    {
        if (_viewModels.Count == 0)
        {
            EmptyStatePanel.Visibility = Visibility.Visible;
            LvSellers.Visibility = Visibility.Collapsed;
        }
        else
        {
            EmptyStatePanel.Visibility = Visibility.Collapsed;
            LvSellers.Visibility = Visibility.Visible;
        }
    }

    private async void OnAddSellerAppBarButtonClicked(object sender, RoutedEventArgs e)
    {
        if (_currentSellerViewModel == null && SvSeller.Visibility == Visibility.Visible)
        {
            var result = await this.ShowMessageDialogAsync(Constants.MessageDialogWarning, Localization.GetLocalizedString("/SellersPage/MessageDialogDiscardAddWarningMessage"), Constants.MessageDialogYes, Constants.MessageDialogNo);
            if (result != ContentDialogResult.Primary) return;
        }
        else if (_currentSellerViewModel != null)
        {
            var result = await this.ShowMessageDialogAsync(Constants.MessageDialogWarning, Localization.GetLocalizedString("/SellersPage/MessageDialogDiscardEditWarningMessage"), Constants.MessageDialogYes, Constants.MessageDialogNo);
            if (result != ContentDialogResult.Primary) return;
        }

        LvSellers.SelectedItem = null;
        _currentSellerViewModel = null;

        TbxName.Text = "";
        TbxContact.Text = "";
        BtSave.Content = Localization.GetLocalizedString("/SellersPage/AddButtonContent");

        SvSeller.Visibility = Visibility.Visible;
    }

    private async void OnDeleteSellerAppBarButtonClicked(object sender, RoutedEventArgs e)
    {
        var result = await this.ShowMessageDialogAsync(Constants.MessageDialogWarning, Localization.GetLocalizedString("/SellersPage/MessageDialogDeleteSellerConfirmationMessage"), Constants.MessageDialogYes, Constants.MessageDialogNo);
        if (result != ContentDialogResult.Primary) return;

        var sellerId = _currentSellerViewModel.Seller.Id;
        SellerManager.RemoveSeller(sellerId);
        _viewModels.Remove(_currentSellerViewModel);
        UpdateEmptyState();
    }

    private async void OnSaveSellerButtonClicked(object sender, RoutedEventArgs e)
    {
        var name = TbxName.Text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            await this.ShowMessageDialogAsync(Constants.MessageDialogWarning, Localization.GetLocalizedString("/SellersPage/MessageDialogSellerNameRequiredMessage"), Constants.MessageDialogOk);
            return;
        }

        var isAdd = _currentSellerViewModel == null;
        var seller = _currentSellerViewModel?.Seller ?? new Seller();
        seller.Name = name;
        seller.Contact = TbxContact.Text.Trim();
        SellerManager.SaveSeller(seller);

        if (isAdd)
        {
            var viewModel = new ManageSellerViewModel(seller);
            _viewModels.Add(viewModel);
            _currentSellerViewModel = viewModel;
            LvSellers.SelectedItem = viewModel;
            UpdateEmptyState();
        }
        else _currentSellerViewModel.SetSeller(seller);
    }

    private void OnSellersListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var listView = sender as ListView;
        var selectedItem = listView.SelectedItem as ManageSellerViewModel;

        if (selectedItem == null)
        {
            SvSeller.Visibility = Visibility.Collapsed;
            AbbDelete.IsEnabled = false;
            _currentSellerViewModel = null;
            return;
        }

        _currentSellerViewModel = selectedItem;

        TbxName.Text = selectedItem.Seller.Name;
        TbxContact.Text = selectedItem.Seller.Contact ?? "";
        BtSave.Content = Localization.GetLocalizedString("/SellersPage/SaveButtonContent");

        SvSeller.Visibility = Visibility.Visible;
        AbbDelete.IsEnabled = true;
    }

    private void OnMainGridSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var width = e.NewSize.Width;
        var isWide = width > 650;

        var columnDefinitions = MainGrid.ColumnDefinitions;
        var rowDefinitions = MainGrid.RowDefinitions;
        columnDefinitions.Clear();
        rowDefinitions.Clear();

        if (isWide)
        {
            columnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            columnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
            Grid.SetColumn(SellersGrid, 0);
            Grid.SetColumn(SvSeller, 1);
            SvSeller.CornerRadius = new CornerRadius(10, 0, 0, 10);
        }
        else
        {
            rowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            rowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            Grid.SetRow(SellersGrid, 0);
            Grid.SetRow(SvSeller, 1);
            SvSeller.CornerRadius = new CornerRadius(10, 10, 0, 0);
        }
    }
}
