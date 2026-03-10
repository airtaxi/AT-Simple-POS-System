using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Enums;
using Windows.ApplicationModel.Resources;

namespace App;

public static class Localization
{
#if !HAS_UNO
    private static Microsoft.Windows.ApplicationModel.Resources.ResourceManager s_resourceManager = new();

#endif
    public static Language GetLanguage()
    {
        var currentLanguage = Configuration.GetValue<string>("Language");
        if (string.IsNullOrEmpty(currentLanguage))
        {
            currentLanguage = GetLocalizedString("LanguageName");
            Console.WriteLine($"Language Not Set. Fall back to system language: {currentLanguage}");
            SetLanguage(currentLanguage);
            Console.WriteLine($"language set: {currentLanguage}");
        }

        if (string.IsNullOrEmpty(currentLanguage)) currentLanguage = "English";
        return Enum.Parse<Language>(currentLanguage);
    }

    public static void SetLanguage(string languageName) => Configuration.SetValue("Language", languageName);

    public static string GetLocalizedString(string key)
    {
#if HAS_UNO
        var result = ResourceLoader.GetForViewIndependentUse().GetString(key);
        Console.WriteLine($"GetLocalizedString: {key} => {result}");
        return result;
#else
        // Key format: "/ResourceFileName/KeyName" or just "KeyName"
        var trimmedKey = key.TrimStart('/');

        // If key has no subtree prefix, default to "Resources" subtree
        if (!trimmedKey.Contains('/'))
            trimmedKey = $"Resources/{trimmedKey}";

        var result = s_resourceManager.MainResourceMap.TryGetValue(trimmedKey)?.ValueAsString ?? string.Empty;
        Console.WriteLine($"GetLocalizedString: {key} => {result}");
        return result;
#endif
    }

    public static void OverrideLanguage(Language language)
    {
#if HAS_UNO
        if (language == Language.Korean) Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "ko-KR";
        else if (language == Language.English) Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "en-US";
#else
        if (language == Language.Korean) Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "ko-KR";
        else if (language == Language.English) Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "en-US";
#endif
    }

    public static void SetupDefaultLanguage()
    {
        var language = GetLanguage();
        Debug.WriteLine("Current Language: " + language);
        OverrideLanguage(language);
    }
}
