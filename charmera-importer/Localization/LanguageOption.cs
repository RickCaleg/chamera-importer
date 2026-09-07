using System.Globalization;
using System.Linq;

namespace charmera_importer.Localization;

public sealed record LanguageOption(string Code, string DisplayName)
{
    public static readonly LanguageOption Portuguese = new("pt", "Português");
    public static readonly LanguageOption English = new("en", "English");

    public static readonly LanguageOption[] All = [Portuguese, English];

    public static LanguageOption FromCode(string? code) =>
        All.FirstOrDefault(l => l.Code == code) ?? English;

    // Used only when there's no saved preference yet (first run) — falls back to English
    // for any system locale we don't have a translation for.
    public static LanguageOption DetectSystem()
    {
        var code = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        return All.FirstOrDefault(l => l.Code == code) ?? English;
    }
}
