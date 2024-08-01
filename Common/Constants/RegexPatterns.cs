namespace Translations.Common.Constants;

public static class RegexPatterns
{
    /// <summary>
    /// Regex pattern that splits "The.Key": "(and) The description/Text etc....",
    /// </summary>
    public const string KeyAndTextPattern = @"""([^""]+)"": ""([^""]+)""(,)?";

    /// <summary>
    /// Regex pattern that removed {myComplexImage} in texts",
    /// </summary>
    public const string ComplexStringPattern = @"\{[^}]*\}";

    /// <summary>
    /// Regex pattern that removes special characters in texts",
    /// </summary>
    public const string SpecialCharactersPattern = @"[!@#$%^&*(),.?""{}|<>]";
}
