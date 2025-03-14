#if __IOS__
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Uno.Extensions.Storage.Pickers;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Pickers;
using Microsoft.UI.Xaml.Controls;

namespace App.Extensions;

public class FolderSavePickerExtension : IFileSavePickerExtension
{
    private const string FolderExistsTitleResource = "/FileSavePicker/FolderSavePickerFolderExistsTitle";
    private const string FileExistsTitleResource = "/FileSavePicker/FolderSavePickerFileExistsTitle";
    private const string FolderExistsTextResource = "/FileSavePicker/FolderSavePickerFolderExistsText";
    private const string FileExistsTextResource = "/FileSavePicker/FolderSavePickerFileExistsText";
    private const string CreateFailedTitleResource = "/FileSavePicker/FolderSavePickerCreateFailedTitle";
    private const string CreateFailedTextResource = "/FileSavePicker/FolderSavePickerCreateFailedText";

    private const string YesResource = "/FileSavePicker/FolderSavePickerYes";
    private const string NoResource = "/FileSavePicker/FolderSavePickerNo";
    private const string OkResource = "/FileSavePicker/FolderSavePickerOk";

    private readonly FileSavePicker _fileSavePicker;
    private readonly ResourceLoader _resourceLoader = ResourceLoader.GetForViewIndependentUse();

    public FolderSavePickerExtension(object owner)
    {
        if (owner is not FileSavePicker picker) throw new ArgumentException($"Owner of {nameof(FolderSavePickerExtension)} must be a {nameof(FileSavePicker)}");
        _fileSavePicker = picker;
    }

    public async Task<StorageFile> PickSaveFileAsync(CancellationToken token)
    {
        var dialog = new FolderSavePickerDialog();

        if (!string.IsNullOrEmpty(_fileSavePicker.CommitButtonText))
        {
            dialog.PrimaryButtonText = _fileSavePicker.CommitButtonText;
        }

        if (!string.IsNullOrEmpty(_fileSavePicker.SuggestedFileName))
        {
            dialog.SuggestedFileName = _fileSavePicker.SuggestedFileName;
        }

        dialog.SetFileTypeChoices(_fileSavePicker.FileTypeChoices);           

        do
        {
            await dialog.ShowAsync();

            var pickConfirmed = true;

            var fileName = dialog.PickedFileName;
            var pickedFolder = dialog.PickedFolder;
            if (fileName != null)
            {
                var item = await dialog.PickedFolder.TryGetItemAsync(fileName);
                if (item != null)
                {
                    var itemExistsDialog = new ContentDialog();
                    if (item.IsOfType(StorageItemTypes.Folder))
                    {
                        itemExistsDialog.Title = _resourceLoader.GetString(FolderExistsTitleResource);
                        itemExistsDialog.Content = string.Format(_resourceLoader.GetString(FolderExistsTextResource), fileName, pickedFolder.DisplayName);
                        itemExistsDialog.DefaultButton = ContentDialogButton.Primary;
                        itemExistsDialog.PrimaryButtonText = _resourceLoader.GetString(OkResource);
                        itemExistsDialog.DefaultButton = ContentDialogButton.Primary;
                        await itemExistsDialog.ShowAsync();
                        pickConfirmed = false;
                    }
                    else
                    {
                        itemExistsDialog.Title = _resourceLoader.GetString(FileExistsTitleResource);
                        itemExistsDialog.Content = string.Format(_resourceLoader.GetString(FileExistsTextResource), fileName, pickedFolder.DisplayName);
                        itemExistsDialog.DefaultButton = ContentDialogButton.Primary;
                        itemExistsDialog.PrimaryButtonText = _resourceLoader.GetString(YesResource);
                        itemExistsDialog.SecondaryButtonText = _resourceLoader.GetString(NoResource);
                        itemExistsDialog.DefaultButton = ContentDialogButton.Secondary;
                        var result = await itemExistsDialog.ShowAsync();
                        if (result != ContentDialogResult.Primary)
                        {
                            pickConfirmed = false;
                        }
                    }
                }
            }

            if (pickConfirmed)
            {
                if (dialog.PickedFileName == null)
                {
                    return null;
                }

                try
                {
                    return await dialog.PickedFolder.CreateFileAsync(dialog.PickedFileName, CreationCollisionOption.ReplaceExisting);
                }
                catch (Exception)
                {
                    var saveFailedDialog = new ContentDialog
                    {
                        PrimaryButtonText = _resourceLoader.GetString(OkResource),
                        DefaultButton = ContentDialogButton.Primary,
                        Title = _resourceLoader.GetString(CreateFailedTitleResource),
                        Content = string.Format(
                            _resourceLoader.GetString(CreateFailedTextResource),
                            dialog.PickedFileName,
                            dialog.PickedFolder.DisplayName)
                    };
                    await saveFailedDialog.ShowAsync();
                }
            }
        }
        while (true);
    }
}
#endif
