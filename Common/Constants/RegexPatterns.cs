namespace Translations.Common.Constants;

public static class RegexPatterns
{
    /// <summary>
    /// Regex pattern that splits "The.Key": "(and) The description/Text etc....",
    /// </summary>
    public const string KeyAndTextPattern = @"""([^""]+)"": ""([^""]+)""(,)?";
}
