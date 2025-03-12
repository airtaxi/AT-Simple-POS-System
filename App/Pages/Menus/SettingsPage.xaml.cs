using System.Diagnostics;
using App.Enums;
using Windows.Storage.Pickers;

namespace App.Pages.Menus;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();

        SetLanguageComboBoxSelectedItemToCurrentLanguage();
    }

    private void SetLanguageComboBoxSelectedItemToCurrentLanguage()
    {
        var currentLanguage = Localization.GetLanguage();
        var languageComboBoxItems = LanguageComboBox.Items.Cast<ComboBoxItem>();
        var languages = languageComboBoxItems.Select(x => Enum.Parse<Language>((string)x.Tag)).ToList();
        var languageIndex = languages.IndexOf(currentLanguage);
        LanguageComboBox.SelectedIndex = languageIndex;
    }

    private async void OnLanguageComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var comboBox = sender as ComboBox;
        var selectedComboBoxItem = comboBox.SelectedItem as ComboBoxItem;
        var selectedLanguage = Enum.Parse<Language>((string)selectedComboBoxItem.Tag);

        if(selectedLanguage == Localization.GetLanguage()) return;

        var result = await this.ShowMessageDialogAsync(Constants.MessageDialogWarning, Localization.GetLocalizedString("/SettingsPage/MessageDialogLanguageChangeConfirmationMessage"), Constants.MessageDialogYes, Constants.MessageDialogNo);
        if (result != ContentDialogResult.Primary)
        {
            SetLanguageComboBoxSelectedItemToCurrentLanguage();
            return;
        }

        Localization.SetLanguage(selectedLanguage.ToString());
        Configuration.WriteBuffer();
        Environment.Exit(0);
    }

    private async void OnExportSettingsButtonClicked(object sender, RoutedEventArgs e)
    {
        var savePicker = new FileSavePicker();
#if WINDOWS

        // Get the current window's HWND by passing a Window object
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        // Associate the HWND with the file picker
        WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);
        savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
#endif
        savePicker.FileTypeChoices.Add(Localization.GetLocalizedString("/SettingsPage/AppConfigFileTypeChoice"), [".atspconfig"]);
        savePicker.SuggestedFileName = "Settings";

        var file = await savePicker.PickSaveFileAsync();
        if (file == null) return;

        var jsonText = Configuration.Export();
        await FileIO.WriteTextAsync(file, jsonText);
    }

    private async void OnImportSettingsButtonClicked(object sender, RoutedEventArgs e)
    {
        var result = await this.ShowMessageDialogAsync(Constants.MessageDialogWarning, Localization.GetLocalizedString("/SettingsPage/MessageDialogImportSettingsConfirmationMessage"), Constants.MessageDialogYes, Constants.MessageDialogNo);
        if (result != ContentDialogResult.Primary) return;

        var openPicker = new FileOpenPicker();

#if WINDOWS

        // Get the current window's HWND by passing a Window object
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        // Associate the HWND with the file picker
        WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hwnd);
        openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
#endif
        openPicker.FileTypeFilter.Add(".atspconfig");

        var file = await openPicker.PickSingleFileAsync();
        if (file == null) return;
        var jsonText = await FileIO.ReadTextAsync(file);
        Configuration.Import(jsonText);
    }

    private async void OnClearRecordsButtonClicked(object sender, RoutedEventArgs e)
    {
        var result = await this.ShowMessageDialogAsync(Constants.MessageDialogWarning, Localization.GetLocalizedString("/SettingsPage/MessageDialogClearRecordsConfirmationMessage"), Constants.MessageDialogYes, Constants.MessageDialogNo);
        if (result != ContentDialogResult.Primary) return;

        TransactionManager.ClearTransactions();
    }

    private async void OnClearItemsButtonClicked(object sender, RoutedEventArgs e)
    {
        var result = await this.ShowMessageDialogAsync(Constants.MessageDialogWarning, Localization.GetLocalizedString("/SettingsPage/MessageDialogClearItemsConfirmationMessage"), Constants.MessageDialogYes, Constants.MessageDialogNo);
        if (result != ContentDialogResult.Primary) return;

        ItemManager.ClearItems();
        TransactionManager.ClearTransactions();
    }

    // iOS, Android bug fix
    private void OnUpdateNeedElementLoaded(object sender, RoutedEventArgs e)
    {
        var element = sender as FrameworkElement;
        element.UpdateLayout();
    }
}
