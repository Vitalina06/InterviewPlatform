using InterviewPlatform.Shared;
using InterviewPlatform.Worker;
using QuestPDF.Infrastructure;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

QuestPDF.Settings.License = LicenseType.Community;

var builder = Host.CreateApplicationBuilder(args);

var logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.WithProperty("Service", "worker")
    .WriteTo.Console()
    .WriteTo.File(
        new RenderedCompactJsonFormatter(),
        Path.Combine(builder.Configuration["LogDirectory"]!, "worker-.json"),
        rollingInterval: RollingInterval.Day,
        shared: true)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

builder.Services.AddSingleton<MongoContext>();
builder.Services.AddSingleton<IDocumentStore, MongoDocumentStore>();
builder.Services.AddSingleton<IAuditStore, MongoAuditStore>();
builder.Services.AddSingleton<IPdfGenerator, QuestPdfGenerator>();
builder.Services.AddHostedService<PdfConsumer>();
builder.Services.AddHostedService<AuditConsumer>();

var host = builder.Build();
host.Run();
