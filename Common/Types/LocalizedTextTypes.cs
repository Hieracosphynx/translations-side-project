using Translations.Models;

namespace Translations.Common.Types;

public static class LocalizedTextTypes
{
    public record ParsedTextEntry(string Key = "", string Value = "")
    {
        public string Key { get; set; } = Key;
        public string Value { get; set; } = Value;
    }
    public record FormData(IFormFile[] Files, string? GameFranchise, string? GameName);
    public record Results(List<LocalizedText> FoundTextEntries, List<LocalizedText> NotFoundTextEntries);
    public record FileAndContent(string Filename, Dictionary<string, string> Content);
}