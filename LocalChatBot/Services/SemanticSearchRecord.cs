using Microsoft.Extensions.VectorData;

namespace LocalChatBot.Services;

public class SemanticSearchRecord
{
    [VectorStoreRecordKey]
    public required string Key { get; set; }

    [VectorStoreRecordData]
    public required string FileName { get; set; }

    [VectorStoreRecordData]
    public int PageNumber { get; set; }

    [VectorStoreRecordData]
    public required string Text { get; set; }

    // Default Vector Sizes
    // nomic-embed-text: 768
    // all-minilm: 384
    [VectorStoreRecordVector(384, DistanceFunction.CosineSimilarity)]
    public ReadOnlyMemory<float> Vector { get; set; }
}
