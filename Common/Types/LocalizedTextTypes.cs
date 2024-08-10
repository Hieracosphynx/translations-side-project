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
    public record Results(List<LocalizedTextResult> FoundTextEntries, List<LocalizedText> NotFoundTextEntries);
    public record FileAndContent(string Filename, Dictionary<string, string> Content);

    public sealed class LocalizedTextResult : LocalizedText
    {
        public string OriginalKey { get; set; }
        
        public LocalizedTextResult(LocalizedText localizedText, string originalKey)
        {
            Key = localizedText.Key;
            OriginalKey = originalKey;
            Text = localizedText.Text;
            GameFranchise = localizedText.GameFranchise;
            GameName = localizedText.GameName;
        }
    }
}
