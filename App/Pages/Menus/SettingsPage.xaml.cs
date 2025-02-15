using System.Diagnostics;
using App.Enums;

namespace App.Pages.Menus;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();

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
        if (result != ContentDialogResult.Primary) return;

        Localization.SetLanguage(selectedLanguage.ToString());
        Configuration.WriteBuffer();
        Process.GetCurrentProcess().Kill();
    }
}
