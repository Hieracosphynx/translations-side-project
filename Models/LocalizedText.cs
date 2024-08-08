using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Translations.Common.Enums;

namespace Translations.Models;

public class LocalizedText
{
    public record ParsedTextEntry
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public record FormData
    {
        public IFormFile[] Files { get; set; } = [];
        public string? GameFranchise { get; set; }
        public string? GameName { get; set; }
    }

    public record Results(List<LocalizedText> Found, List<LocalizedText> NotFound)
    {
        public List<LocalizedText> FoundTextEntries { get; set; } = Found;
        public List<LocalizedText> NotFoundTextEntries { get; set; } = NotFound;
    }

    public record FileAndContent(string Filename, Dictionary<string, string> Content)
    {
        public string Filename { get; set; } = Filename;
        public Dictionary<string, string> Content { get; set; } = Content;
    }

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    [BsonElement("Name")]
    public string Key { get; set; } = "This.A.Key"; 
    public string? Text { get; set; } = "";
    public Language.Codes? Language { get; set; } = 0;
    public string? GameFranchise { get; set; } = "";
    public string? GameName { get; set; } = "";
}
