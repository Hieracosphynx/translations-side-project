using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Translations.Common.Enums;

namespace Translations.Models;

public class LocalizedText
{
    public struct ParsedTextEntry
    {
        public string? Key { get; set; }
        public string? Value { get; set; }
    }

    public struct FormData 
    {
        public IFormFile[]? Files { get; set; }
        public string? GameFranchise { get; set; }
        public string? GameName { get; set; }
    }

    public struct Results(List<LocalizedText> found, List<LocalizedText> notFound)
    {
        public List<LocalizedText>? FoundTextEntries { get; set; } = found;
        public List<LocalizedText>? NotFoundTextEntries { get; set; } = notFound;
    }

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    [BsonElement("Name")]
    public string Key { get; set; } = "This.A.Key"; 
    public string? Text { get; set; }
    public Language.Codes? Language { get; set; }
    public string? GameFranchise { get; set; }
    public string? GameName { get; set; }
}
