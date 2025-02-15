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
    public static Language GetLanguage()
    {
        var currentLanguage = Configuration.GetValue("Language") as string;
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
        var result = ResourceLoader.GetForViewIndependentUse().GetString(key);
        Console.WriteLine($"GetLocalizedString: {key} => {result}");
        return result;
    }

    public static void OverrideLanguage(Language language)
    {
#if HAS_UNO
        if (language == Language.Korean) Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "ko";
        else if (language == Language.English) Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "en";
#endif
    }

    public static void SetupDefaultLanguage()
    {
        var language = GetLanguage();
        Debug.WriteLine("Current Language: " + language);
        OverrideLanguage(language);
    }
}
