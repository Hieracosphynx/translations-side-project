using System;
using System.Text.RegularExpressions;

namespace Translations.Common.Utilities;

public static class RegexTools 
{
    public struct ParsedTextEntry
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public static ParsedTextEntry GetParsedTextEntry(string text, string pattern)
    {
        ParsedTextEntry parsedTextEntry= new ParsedTextEntry();

        Console.WriteLine(nameof(GetParsedTextEntry));

        foreach(Match match in Regex.Matches(text, pattern))
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

    public static string PreProcessString(string? input)
    {
        if(input == null || input == "") { return ""; }

        return Regex.Replace(input, @"\{[^}]*\}", string.Empty).Trim();
    }
}