using System.Collections.ObjectModel;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using App.ViewModels;
using CommunityToolkit.WinUI;
using ImageMagick;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Pickers;

namespace App.Pages.Menus;

public sealed partial class ManagePage : Page
{
    private readonly ObservableCollection<ManageItemViewModel> _viewModels = new();
    private ManageItemViewModel _currentItemViewModel;
    private byte[] _currentItemImageBinary;

    public ManagePage()
    {
        this.InitializeComponent();

        // Setup items
        var items = ItemManager.GetItems();
        var viewModels = items.Select(item => new ManageItemViewModel(item));
        foreach (var viewModel in viewModels) _viewModels.Add(viewModel);
        LvItems.ItemsSource = _viewModels;
    }


    private async void OnAddItemAppBarButtonClicked(object sender, RoutedEventArgs e)
    {
        if (_currentItemViewModel == null && SvItem.Visibility == Visibility.Visible)
        {
            var result = await this.ShowMessageDialogAsync("Warning", "You're currently adding an item. Do you want to discard the changes?", Constants.MessageDialogYes, Constants.MessageDialogNo);
            if (result != ContentDialogResult.Primary) return;
        }
        else if (_currentItemViewModel != null)
        {
            var result = await this.ShowMessageDialogAsync("Warning", "You're currently editing an item. Do you want to discard the changes?", Constants.MessageDialogYes, Constants.MessageDialogNo);
            if (result != ContentDialogResult.Primary) return;
        }

        // Reset the selected item
        LvItems.SelectedItem = null;

        // Reset the current item view model and image binary
        _currentItemViewModel = null;
        _currentItemImageBinary = null;

        // Setup the UI
        ImgItem.Source = null;
        TbxName.Text = "";
        TbxPrice.Text = "0";
        TbxPrice.Tag = new object();
        NbxStockQuantity.Value = 0;
        BtSave.Content = "Add";

        // Show the item view
        SvItem.Visibility = Visibility.Visible;
    }

    private async void OnDeleteItemAppBarButtonClicked(object sender, RoutedEventArgs e)
    {
        var result = await this.ShowMessageDialogAsync("Warning", "Are you sure you want to delete this item?", Constants.MessageDialogYes, Constants.MessageDialogNo);
        if (result != ContentDialogResult.Primary) return;

        ItemManager.RemoveItem(_currentItemViewModel.Item.Id);
        _viewModels.Remove(_currentItemViewModel);
    }

    private void OnSaveItemButtonClicked(object sender, RoutedEventArgs e)
    {
        var isAdd = _currentItemViewModel == null;

        var item = _currentItemViewModel?.Item ?? new();
        item.Name = TbxName.Text;
        item.ImageBinary = _currentItemImageBinary ?? File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Assets/Placeholder.png"));
        var priceText = TbxPrice.Text;
        var numberOnlyText = NumberOnlyRegex().Replace(priceText, "");
        item.Price = int.Parse(numberOnlyText);
        item.StockQuantity = (int)NbxStockQuantity.Value;
        ItemManager.SaveItem(item);

        if (isAdd)
        {
            var viewModel = new ManageItemViewModel(item);
            _viewModels.Add(viewModel);
            _currentItemViewModel = viewModel;
            LvItems.SelectedItem = viewModel;
            LvItems.SmoothScrollIntoViewWithItemAsync(viewModel);
        }
        else _currentItemViewModel.SetItem(item);
    }

    private async void OnSetupImageButtonClicked(object sender, RoutedEventArgs e)
    {
        // Create a file picker
        var openPicker = new FileOpenPicker();

#if WINDOWS
        // Retrieve the window handle (HWND) of the current WinUI 3 window.
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);

        // Initialize the file picker with the window handle (HWND).
        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);
#endif

        // Set options for your file picker
        openPicker.ViewMode = PickerViewMode.Thumbnail;
        openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
        openPicker.FileTypeFilter.Add(".jpg");
        openPicker.FileTypeFilter.Add(".jpeg");
        openPicker.FileTypeFilter.Add(".png");
        openPicker.FileTypeFilter.Add(".webp");

        // Open the picker for the user to pick a file
        var file = await openPicker.PickSingleFileAsync();
        if (file == null) return;

        var buffer = await FileIO.ReadBufferAsync(file);

        MainPage.ShowLoading("Processing image...");
        try
        {
            using var memoryStream = new MemoryStream();
            using var stream = buffer.AsStream();
            using var image = new MagickImage(stream);
            image.Resize(0, 192);
            await image.WriteAsync(memoryStream);

            var bitmapImage = new BitmapImage();
            memoryStream.Seek(0, SeekOrigin.Begin);
            bitmapImage.SetSource(memoryStream.AsRandomAccessStream());
            ImgItem.Source = bitmapImage;

            memoryStream.Seek(0, SeekOrigin.Begin);
            _currentItemImageBinary = memoryStream.ToArray();
        }
        finally { MainPage.HideLoading(); }
    }

    private async void OnItemsListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_currentItemViewModel == null && SvItem.Visibility == Visibility.Visible)
        {
            var result = await this.ShowMessageDialogAsync("Warning", "You're currently adding an item. Do you want to discard the changes?", Constants.MessageDialogYes, Constants.MessageDialogNo);
            if (result != ContentDialogResult.Primary) return;
        }

        var listView = sender as ListView;
        var selectedItem = listView.SelectedItem as ManageItemViewModel;

        // If no item is selected
        if (selectedItem == null)
        {
            // Update the UI
            SvItem.Visibility = Visibility.Collapsed;
            AbbDelete.IsEnabled = false;

            // Reset the current item view model and image binary
            _currentItemViewModel = null;
            _currentItemImageBinary = null;

            return;
        }

        // Reset the current item view model and image binary
        _currentItemViewModel = selectedItem;
        _currentItemImageBinary = selectedItem.Item.ImageBinary;

        // Setup the UI
        TbxName.Text = selectedItem.Item.Name;
        // Set the image if available
        if (selectedItem.Item.ImageBinary != null) ImgItem.Source = Utils.GetBitmapImageFromByteArray(selectedItem.Item.ImageBinary);

        TbxPrice.Text = selectedItem.Item.Price.ToString("N0");
        TbxPrice.Tag = new object();
        NbxStockQuantity.Value = selectedItem.Item.StockQuantity;
        BtSave.Content = "Save";

        // Show the item view
        SvItem.Visibility = Visibility.Visible;

        // Enable the delete button
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
            Grid.SetColumn(ItemsGrid, 0);
            Grid.SetColumn(SvItem, 1);
            SvItem.CornerRadius = new CornerRadius(10, 0, 0, 10);
        }
        else
        {
            rowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            rowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            Grid.SetRow(ItemsGrid, 0);
            Grid.SetRow(SvItem, 1);
            SvItem.CornerRadius = new CornerRadius(10, 10, 0, 0);
        }
    }

    private void OnMoneyTextBoxTextChanged(object sender, TextChangedEventArgs e)
    {
        var textBox = sender as TextBox;

        if (textBox.Tag != null) // Return if the text is being set programmatically
        {
            textBox.Tag = null;
            return;
        }

        // Remove non-numeric characters
        var numberOnlyText = NumberOnlyRegex().Replace(textBox.Text, "");
        if (!long.TryParse(numberOnlyText, out long receivedMoney)) return; // Return if the text is not a number

        // Format the number
        var formattedMoneyText = receivedMoney.ToString("N0");
        textBox.Text = formattedMoneyText; // Set the text

        // Set the cursor position to keep the cursor before currency symbol
        textBox.SelectionStart = formattedMoneyText.Length;
        textBox.SelectionLength = 0;
    }

    [GeneratedRegex("[^0-9]")]
    private static partial Regex NumberOnlyRegex();

    // Uno bug workaround
    private async void OnImageFailed(object sender, ExceptionRoutedEventArgs e)
    {
        await Task.Delay(100);
        var image = sender as Image;
        var viewModel = image.DataContext as ManageItemViewModel;
        viewModel.LoadImage();
    }
}
