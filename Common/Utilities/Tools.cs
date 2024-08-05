using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace Translations.Common.Utilities;
public static class Tools 
{
    public static string FormatDictionaryToJson(Dictionary<string, string> dict)
    {
        string jsonString = RegexTools.ParseUnicodeString(
            JsonSerializer.Serialize(dict));
        string formattedJsonString = JToken.Parse(jsonString)
            .ToString(Newtonsoft.Json.Formatting.Indented);

        return formattedJsonString;
    }
}