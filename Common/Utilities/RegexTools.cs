using System.Text.RegularExpressions;
using Translations.Common.Constants;

namespace Translations.Common.Utilities;

public static partial class RegexTools 
{
    public struct ParsedTextEntry
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    /// <summary>
    /// Separates "The.Key": "From the description."
    /// </summary>
    /// <param name="text"></param>
    /// <param name="pattern"></param>
    /// <returns>ParsedTextEntry { Key, Value }</returns>
    public static ParsedTextEntry ParseTextEntry(string text)
    {
        ParsedTextEntry parsedTextEntry = new();

        foreach(Match match in ParseTextEntryRegex().Matches(text))
        {
            if(match.Success)
            {
                string key = match.Groups[1].Value;
                string value = match.Groups[2].Value;

                parsedTextEntry.Key = key;
                parsedTextEntry.Value = value;
            }
            else
            {
                Console.WriteLine("No match found.");
            }
        }

        return parsedTextEntry;
    }

    /// <summary>
    /// General replacing whatever pattern is given and remove white spaces.
    /// </summary>
    /// <param name="input">string to be parsed</param>
    /// <param name="pattern">Regex pattern to follow</param>
    /// <returns></returns>
    public static string PreProcessString(string? input, string pattern)
    {
        if(input == null || input == "") { return ""; }

        return string.Join("", Regex.Replace(input, pattern, string.Empty).Split(' '));
    }

    public static bool IsMatch(string? s1, string? s2, string[] patterns)
    {
        var preProcessedS1 = "";
        var preProcessedS2 = "";

        foreach(var pattern in patterns)
        {
            preProcessedS1 = PreProcessString(s1, pattern);
            preProcessedS2 = PreProcessString(s2, pattern);    
        }

        return preProcessedS1.Equals(preProcessedS2, StringComparison.CurrentCultureIgnoreCase);
    }

    [GeneratedRegex(RegexPatterns.KeyAndTextPattern)]
    private static partial Regex ParseTextEntryRegex();
}