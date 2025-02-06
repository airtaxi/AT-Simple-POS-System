using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace App;

public static class DialogHelper
{
    private static ContentDialog s_lastContentDialog;
    public static ContentDialog GenerateMessageDialog(this UIElement element, string title, string content, string primaryButtonText = Constants.MessageDialogOk, string secondaryButtonText = null)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            PrimaryButtonText = primaryButtonText
        };
        if (secondaryButtonText != null) dialog.SecondaryButtonText = secondaryButtonText;
        dialog.XamlRoot = element.XamlRoot;
        return dialog;
    }

    public static async Task<ContentDialogResult> ShowMessageDialogAsync(this UIElement element, string title, string content, string primaryButtonText = Constants.MessageDialogOk, string secondaryButtonText = null)
    {
        var dialog = element.GenerateMessageDialog(title, content, primaryButtonText, secondaryButtonText);
        s_lastContentDialog?.Hide();
        var result = await dialog.ShowAsync();
        s_lastContentDialog = dialog;
        return result;
    }
}
