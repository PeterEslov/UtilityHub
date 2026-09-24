using OutageWatch.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<OutageStore>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "OutageWatch.Api" }));

app.MapGet("/api/outages", (OutageStore store) => Results.Ok(store.GetAll()));

app.MapGet("/api/outages/active", (OutageStore store) => Results.Ok(store.GetActive()));

app.MapPost("/api/outages", (CreateOutageRequest request, OutageStore store) =>
{
    var created = store.Add(request);
    // TODO (Fas 2): publicera en händelse på Event Grid här istället för
    // att bara lagra lokalt, så CustomerDesk kan reagera händelsedrivet.
    return Results.Created($"/api/outages/{created.Id}", created);
});

app.Run();
