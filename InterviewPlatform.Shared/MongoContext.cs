using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace InterviewPlatform.Shared;

public class MongoContext
{
    public IMongoCollection<DocumentRecord> Documents { get; }
    public IMongoCollection<AuditRecord> Audit { get; }

    public MongoContext(IConfiguration config)
    {
        var client = new MongoClient(config["Mongo:ConnectionString"]);
        var database = client.GetDatabase(config["Mongo:Database"]);

        Documents = database.GetCollection<DocumentRecord>("documents");
        Audit = database.GetCollection<AuditRecord>("audit");
    }
}
