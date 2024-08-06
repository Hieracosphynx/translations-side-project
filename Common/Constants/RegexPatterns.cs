namespace Translations.Common.Constants;

public static class RegexPatterns
{
    /// <summary>
    /// Regex pattern that splits "The.Key": "(and) The description/Text etc....",
    /// </summary>
    public const string KeyAndTextPattern = @"""([^""]+)"": ""([^""]+)""(,)?";

    /// <summary>
    /// Regex pattern that represents {myComplexImage} and |||Untranslated Part||| in texts",
    /// </summary>
    public const string ComplexStringPattern = @"\{[^}]*\}|\|\|\|.*?\|\|\|";

    /// <summary>
    /// Regex pattern that represents special characters in texts.
    /// </summary>
    public const string SpecialCharactersPattern = @"[!@#$%^&*(),.?""{}|<>]";

    /// <summary>
    /// Regex pattern that represents special characters except {}
    /// </summary>
    public const string SpecialCharExceptBraces = @"[!@#$%^&*(),.?""|<>]";

    /// <summary>
    /// Regex pattern that check for unicode characters in a string. 
    /// </summary>
    public const string UnicodeStringPattern = @"\\[Uu]([0-9A-Fa-f]{4})";
}
