using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace Translations.Common.Utilities;
public static class Tools 
{
    public static string FormatDictionaryToJson(Dictionary<string, string> dict)
    {
        return RegexTools.ParseUnicodeString(JsonSerializer.Serialize(dict));
    }

    public static string ParseJsonToReadable(string jsonString)
    {
        return JToken.Parse(jsonString).ToString(Newtonsoft.Json.Formatting.Indented);
    }
}