using MeterCore.Api;

var builder = WebApplication.CreateBuilder(args);

// Application Insights kopplas bara in om en connection string faktiskt finns
// (satt via miljövariabeln APPLICATIONINSIGHTS_CONNECTION_STRING). Utan den
// no-opar SDK:t tyst lokalt istället för att fylla loggen med varningar.
if (!string.IsNullOrWhiteSpace(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
{
    builder.Services.AddApplicationInsightsTelemetry();
}

builder.Services.AddSingleton<MeterReadingStore>();
builder.Services.AddSingleton<ServiceBusPublisher>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "MeterCore.Api" }));

app.MapGet("/api/meterreadings", (MeterReadingStore store) => Results.Ok(store.GetAll()));

app.MapGet("/api/meterreadings/{meterId}", (string meterId, MeterReadingStore store) =>
{
    var readings = store.GetByMeterId(meterId).ToList();
    return readings.Count == 0 ? Results.NotFound() : Results.Ok(readings);
});

app.MapPost("/api/meterreadings", async (CreateMeterReadingRequest request, MeterReadingStore store, ServiceBusPublisher publisher, ILogger<Program> logger) =>
{
    var created = store.Add(request);

    // Transform-and-forward, steg 1: publicera händelsen. MeterCore känner
    // inte till att BillFlow ens existerar — det är hela poängen. Om
    // publiceringen misslyckas (t.ex. kön är nere) loggar vi och svarar ändå
    // 201, för mätvärdet är redan sparat lokalt. En produktionsversion skulle
    // hantera detta med en outbox-pattern, men det är ett medvetet undantag
    // härifrån för att hålla Fas 1 hanterbar.
    try
    {
        await publisher.PublishMeterReadingRecordedAsync(new MeterReadingRecordedMessage(
            created.MeterId, created.CustomerId, created.Timestamp, created.ConsumptionKwh, created.IsEstimated));
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Kunde inte publicera MeterReadingRecordedMessage för mätare {MeterId}", created.MeterId);
    }

    return Results.Created($"/api/meterreadings/{created.MeterId}", created);
});

app.Run();
