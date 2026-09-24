using CustomerDesk.Api;

var builder = WebApplication.CreateBuilder(args);

if (!string.IsNullOrWhiteSpace(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
{
    builder.Services.AddApplicationInsightsTelemetry();
}

builder.Services.AddSingleton<CustomerDeskStore>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "CustomerDesk.Api" }));

app.MapGet("/api/customers", (CustomerDeskStore store) => Results.Ok(store.GetCustomers()));

app.MapGet("/api/cases", (CustomerDeskStore store) => Results.Ok(store.GetCases()));

app.MapPost("/api/cases", (CreateCaseRequest request, CustomerDeskStore store) =>
{
    var created = store.AddCase(request);
    return Results.Created($"/api/cases/{created.Id}", created);
});

app.Run();
