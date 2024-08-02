namespace Translations.Common.Enums;

public static class Language
{
    // TODO: Missing language codes
    public enum Codes
    {
        Unknown,
        bg_BG,
        ca_ES,
        cs_CZ,
        en_US,
        en_GB,
        ja_JP,
        ro_RO
    }

    public static Codes GetLanguageCodeEnum(string fileName)
    {
        return Enum.TryParse(fileName, true, out Codes code) 
            ? code 
            : Codes.Unknown;
    }
}