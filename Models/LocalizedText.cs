using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Translations.Common.Enums;

namespace Translations.Models;

public class LocalizedText
{
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
