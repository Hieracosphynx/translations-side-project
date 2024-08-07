using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Translations.Common.Enums;

namespace Translations.Models;

public class LocalizedText
{
    public class ParsedTextEntry
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public class FormData
    {
        public IFormFile[] Files { get; set; } = [];
        public string? GameFranchise { get; set; }
        public string? GameName { get; set; }
    }

    public class Results(List<LocalizedText> found, List<LocalizedText> notFound)
    {
        public List<LocalizedText> FoundTextEntries { get; set; } = found;
        public List<LocalizedText> NotFoundTextEntries { get; set; } = notFound;
    }

    public class FileAndContent(string filename, string content)
    {
        public string Filename { get; set; } = filename;
        public string Content { get; set; } = content;
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
