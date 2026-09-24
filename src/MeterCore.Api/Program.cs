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

// Trasig eller ofullständig JSON i request-body:n misslyckas redan under
// model binding — INNAN vår egen endpoint-kod ens körs. Det felet går
// därför inte att fånga med ett try/catch inne i en handler; det måste
// hanteras här, på pipeline-nivå. Utan det här returnerar ASP.NET Core i
// Development-läge en HTML-felsida med hela stacktracen (det du såg
// tidigare) istället för ett rent 400-svar ett API borde ge.
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(new
        {
            title = "Ogiltig förfrågan",
            status = 400,
            detail = "Kunde inte tolka request-body. Kontrollera att JSON är giltig och innehåller alla obligatoriska fält."
        });
    });
});

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
    // Det här fångar den ANDRA sortens fel: JSON:en gick fint att tolka,
    // men värdena i den bryter mot affärsreglerna (tomt MeterId, negativ
    // förbrukning). Det är alltid en bugg i den anropande koden om detta
    // triggas — därför Results.ValidationProblem(), inte en logg-rad.
    var validationErrors = request.Validate();
    if (validationErrors.Count > 0)
    {
        return Results.ValidationProblem(validationErrors);
    }

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
