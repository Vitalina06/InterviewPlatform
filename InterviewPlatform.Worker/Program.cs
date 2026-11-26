using InterviewPlatform.Shared;
using InterviewPlatform.Worker;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<MongoContext>();
builder.Services.AddSingleton<IDocumentStore, MongoDocumentStore>();
builder.Services.AddSingleton<IAuditStore, MongoAuditStore>();
builder.Services.AddSingleton<IPdfGenerator, QuestPdfGenerator>();
builder.Services.AddHostedService<PdfConsumer>();
builder.Services.AddHostedService<AuditConsumer>();

var host = builder.Build();
host.Run();
