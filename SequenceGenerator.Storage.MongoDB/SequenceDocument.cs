using MongoDB.Bson.Serialization.Attributes;

namespace SequenceGenerator.Storage.MongoDB;

[BsonIgnoreExtraElements]
public class SequenceDocument<TElem>
{
    public string Name { get; set; }

    public List<TElem> Elements { get; set; } = new();
}
