using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Translations.Common.Enums;

namespace Translations.Models;

public class LocalizedText
{
    public struct LocalizedTextFormData
    {
        public LanguageCodes? Language { get; set; }
        public string? GameFranchise { get; set; }
        public string? GameName { get; set; }
    }

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    [BsonElement("Name")]
    public string Key { get; set; } = "This.A.Key"; 
    public string? Text { get; set; }
    public LanguageCodes? Language { get; set; }
    public string? GameFranchise { get; set; }
    public string? GameName { get; set; }
}
