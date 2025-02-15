using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using App.DataTypes;
using App.ViewModels;
using Microsoft.UI.Xaml.Input;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace App.Pages.Menus;
/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ItemsPage : Page
{
    private ObservableCollection<TransactionViewModel> TransactionViewModels => IrTransactions.ItemsSource as ObservableCollection<TransactionViewModel>;
    private ObservableCollection<ItemViewModel> ItemViewModels => IrItems.ItemsSource as ObservableCollection<ItemViewModel>;

    public ItemsPage()
    {
        this.InitializeComponent();

        var items = ItemManager.GetItems();
        var itemViewModels = new ObservableCollection<ItemViewModel>(items.Select(item => new ItemViewModel(item.Id)));
        IrItems.ItemsSource = itemViewModels;

        var transactionViewModels = new ObservableCollection<TransactionViewModel>();
        IrTransactions.ItemsSource = transactionViewModels;
    }

    private void UpdateTotal()
    {
        // Update the total quantity
        var totalQuantity = TransactionViewModels.Sum(x => x.Quantity);
        TbTotalQuantity.Text = $"{totalQuantity}";

        // Update the total price
        var totalPrice = TransactionViewModels.Sum((Func<TransactionViewModel, int>)(x => (int)(x.Item.Price * x.Quantity)));
        TbTotalPrice.Text = $"{totalPrice:N0}";

        // Parse the received money text
        var moneyReceivedText = TbxMoneyReceived.Text;
        var numberOnlyText = NumberOnlyRegex().Replace(moneyReceivedText, "");
        // Return if the text is not a number
        if (!long.TryParse(numberOnlyText, out long moneyReceivd))
        {
            TbxChange.Text = string.Empty;
            return;
        }

        // Update the UI
        if (moneyReceivd >= totalPrice) TbxChange.Text = $"{moneyReceivd - totalPrice:N0}";
        else TbxChange.Text = Localization.GetLocalizedString("/ItemsPage/NotEnoughMoneyText");
    }

    private void OnTransactionDeleted(object sender, EventArgs e)
    {
        // Remove the transaction view model
        var viewModel = sender as TransactionViewModel;
        TransactionViewModels.Remove(viewModel);

        // Update the total since the transaction is deleted
        UpdateTotal();
    }

    // Update the total when the quantity of the transaction is changed
    private void OnTransactionQuantityChanged(object sender, EventArgs e) => UpdateTotal();

    // Format the money text
    private void OnMoneyReceivedTextBoxTextChanged(object sender, TextChangedEventArgs e)
    {
        var textBox = sender as TextBox;

        // Remove non-numeric characters
        var numberOnlyText = NumberOnlyRegex().Replace(textBox.Text, "");
        if (!long.TryParse(numberOnlyText, out long moneyReceived)) return; // Return if the text is not a number

        // Format the number
        var formattedMoneyText = moneyReceived.ToString("N0");
        textBox.Text = formattedMoneyText; // Set the text

        // Set the cursor position to keep the cursor before currency symbol
        textBox.SelectionStart = formattedMoneyText.Length;
        textBox.SelectionLength = 0;

        // Update the change since the received money is changed
        UpdateTotal();
    }

    private void OnClearItemsButtonClicked(object sender, RoutedEventArgs e)
    {
        TransactionViewModels.Clear();

        // Update the total since the items are cleared
        UpdateTotal();
    }

    private async void OnPayButtonClicked(object sender, RoutedEventArgs e)
    {
        if(TransactionViewModels.Count == 0)
        {
            await this.ShowMessageDialogAsync(Constants.MessageDialogError, Localization.GetLocalizedString("/ItemsPage/MessageDialogNoItemsErrorMessage"), Constants.MessageDialogOk);
            return;
        }

        var moneyReceivedText = TbxMoneyReceived.Text;
        var numberOnlyText = NumberOnlyRegex().Replace(moneyReceivedText, "");
        _ = long.TryParse(numberOnlyText, out long moneyReceivd);

        var totalPrice = TransactionViewModels.Sum((Func<TransactionViewModel, int>)(x => (int)(x.Item.Price * x.Quantity)));

        if (moneyReceivd < totalPrice)
        {
            var result = await this.ShowMessageDialogAsync(Constants.MessageDialogWarning, Localization.GetLocalizedString("/ItemsPage/MessageDialogNotEnoughMoneyWarningMessage"), Constants.MessageDialogYes, Constants.MessageDialogNo);
            if (result != ContentDialogResult.Primary) return;
        }

        // Setup the record
        var timestamp = DateTime.UtcNow; // Uses UTC time to prevent time zone issues
        var recordId = Guid.NewGuid().ToString("N");
        var transactions = TransactionViewModels.Select((Func<TransactionViewModel, Transaction>)(x => new Transaction(recordId, x.Item.Id, (int)x.Quantity, timestamp)));
        TransactionManager.AddTransactions(transactions); // Add the transactions to the database

        // Clear the transaction view models
        TransactionViewModels.Clear();

        // Refresh the item view models to update the quantity
        foreach (var itemViewModel in ItemViewModels) itemViewModel.Refresh();

        // Show a success dialog
        await this.ShowMessageDialogAsync(Constants.MessageDialogSuccess, Localization.GetLocalizedString("/ItemsPage/MessageDialogPaySuccessMessage"), Constants.MessageDialogOk);

        // Reset the UI
        TbxMoneyReceived.Text = "";
        TbxChange.Text = "";
        UpdateTotal();
    }

    private void OnItemTapped(object sender, TappedRoutedEventArgs e)
    {
        var itemViewModel = ((FrameworkElement)sender).DataContext as ItemViewModel;

        var items = ItemManager.GetItems();
        var item = items.FirstOrDefault(x => x.Id == itemViewModel.ItemId);

        var existingViewModel = TransactionViewModels.FirstOrDefault(x => x.Item.Id == item.Id);
        if (existingViewModel is not null)
        {
            existingViewModel.IncreaseQuantity();
            existingViewModel.InvokeQuantityChangedEvent();
            return;
        }

        var transactionViewModel = new TransactionViewModel(item);
        transactionViewModel.Deleted += OnTransactionDeleted;
        transactionViewModel.QuantityChanged += OnTransactionQuantityChanged;
        transactionViewModel.ToggleWideMode(_isWide);
        TransactionViewModels.Add(transactionViewModel); // Add the transaction view model

        // Update the total since the transaction is added
        UpdateTotal();
    }

    [GeneratedRegex("[^0-9]")]
    private static partial Regex NumberOnlyRegex();

    private bool _isWide;
    private void OnMainGridSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var width = e.NewSize.Width;
        _isWide = width > 650;

        var columnDefinitions = MainGrid.ColumnDefinitions;
        var rowDefinitions = MainGrid.RowDefinitions;
        columnDefinitions.Clear();
        rowDefinitions.Clear();

        if (_isWide)
        {
            columnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            columnDefinitions.Add(new ColumnDefinition { Width = new GridLength(500, GridUnitType.Pixel) });
            Grid.SetColumn(SvItems, 0);
            Grid.SetColumn(GdPay, 1);
        }
        else
        {

            rowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            rowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            Grid.SetRow(SvItems, 0);
            Grid.SetRow(GdPay, 1);
        }

        GdPay.CornerRadius = _isWide ? new CornerRadius(10, 0, 0, 10) : new CornerRadius(10, 10, 0, 0);

        QuantityColumnDefinition.Width = _isWide ? new GridLength(130, GridUnitType.Pixel) : new GridLength(60, GridUnitType.Pixel);
        foreach (var viewModel in TransactionViewModels) viewModel.ToggleWideMode(_isWide);
    }

    // Uno bug workaround
    private async void OnImageFailed(object sender, ExceptionRoutedEventArgs e)
    {
        await Task.Delay(100);
        var image = sender as Image;
        var viewModel = image.DataContext as ItemViewModel;
        viewModel.Refresh();
    }
}
