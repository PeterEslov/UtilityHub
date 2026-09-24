using MeterCore.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<MeterReadingStore>();
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

app.MapPost("/api/meterreadings", (CreateMeterReadingRequest request, MeterReadingStore store) =>
{
    var created = store.Add(request);
    return Results.Created($"/api/meterreadings/{created.MeterId}", created);
});

app.Run();
