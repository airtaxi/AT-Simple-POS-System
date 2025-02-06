using System.Security.Cryptography;
using Microsoft.UI.Xaml.Media.Imaging;


namespace App;

public static class Utils
{
    private readonly static string BasePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private readonly static string TempDirectory = Path.Combine(BasePath, "Temp");

    public static string ComputeSha256Checksum(byte[] byteArray)
    {
        var hashBytes = SHA256.HashData(byteArray);
        return Convert.ToHexStringLower(hashBytes);
    }

    public static BitmapImage GetBitmapImageFromByteArray(byte[] byteArray)
    {
        var bitmap = new BitmapImage();
        using var memoryStream = new MemoryStream();
        memoryStream.Write(byteArray, 0, byteArray.Length);
        memoryStream.Seek(0, SeekOrigin.Begin);
        bitmap.SetSource(memoryStream.AsRandomAccessStream());
        return bitmap;
    }
}
