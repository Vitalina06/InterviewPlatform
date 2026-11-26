using MongoDB.Driver;

namespace InterviewPlatform.Shared;

public class AuditRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Action { get; set; } = "";
    public string UserLogin { get; set; } = "";
    public string Details { get; set; } = "";
    public DateTime Time { get; set; }
}

public interface IAuditStore
{
    Task AddAsync(AuditRecord record);
}

public class MongoAuditStore : IAuditStore
{
    private readonly IMongoCollection<AuditRecord> _audit;

    public MongoAuditStore(MongoContext context)
    {
        _audit = context.Audit;
    }

    public async Task AddAsync(AuditRecord record)
    {
        await _audit.InsertOneAsync(record);
    }
}
