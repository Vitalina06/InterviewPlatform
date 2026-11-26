using MongoDB.Driver;

namespace InterviewPlatform.Shared;

// запись о PDF-документе в MongoDB
public class DocumentRecord
{
    public string Id { get; set; } = "";
    public DocumentType Type { get; set; }
    public string FileName { get; set; } = "";
    public DocumentStatus Status { get; set; }
    public byte[]? Content { get; set; }
    public string? Error { get; set; }
    public DateTime CreatedAt { get; set; }
}

public interface IDocumentStore
{
    Task CreatePendingAsync(DocumentRecord record);
    Task MarkReadyAsync(string id, byte[] content);
    Task MarkFailedAsync(string id, string error);
    Task<DocumentRecord?> GetAsync(string id);
}

public class MongoDocumentStore : IDocumentStore
{
    private readonly IMongoCollection<DocumentRecord> _documents;

    public MongoDocumentStore(MongoContext context)
    {
        _documents = context.Documents;
    }

    public async Task CreatePendingAsync(DocumentRecord record)
    {
        await _documents.InsertOneAsync(record);
    }

    public async Task MarkReadyAsync(string id, byte[] content)
    {
        var update = Builders<DocumentRecord>.Update
            .Set(d => d.Status, DocumentStatus.Ready)
            .Set(d => d.Content, content);

        await _documents.UpdateOneAsync(d => d.Id == id, update);
    }

    public async Task MarkFailedAsync(string id, string error)
    {
        var update = Builders<DocumentRecord>.Update
            .Set(d => d.Status, DocumentStatus.Failed)
            .Set(d => d.Error, error);

        await _documents.UpdateOneAsync(d => d.Id == id, update);
    }

    public async Task<DocumentRecord?> GetAsync(string id)
    {
        return await _documents.Find(d => d.Id == id).FirstOrDefaultAsync();
    }
}
