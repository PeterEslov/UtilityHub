using BillFlow.Api;

var builder = WebApplication.CreateBuilder(args);

if (!string.IsNullOrWhiteSpace(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
{
    builder.Services.AddApplicationInsightsTelemetry();
}

builder.Services.AddSingleton<InvoiceStore>();
builder.Services.AddSingleton<UsageStore>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "BillFlow.Api" }));

app.MapGet("/api/invoices", (InvoiceStore store) => Results.Ok(store.GetAll()));

app.MapGet("/api/invoices/{customerId}", (string customerId, InvoiceStore store) =>
{
    var invoices = store.GetByCustomerId(customerId).ToList();
    return invoices.Count == 0 ? Results.NotFound() : Results.Ok(invoices);
});

app.MapPost("/api/invoices", (CreateInvoiceRequest request, InvoiceStore store) =>
{
    var created = store.Add(request);
    return Results.Created($"/api/invoices/{created.CustomerId}", created);
});

// Transform-and-forward, steg 2: det här är mottagarsidan. Gateway-funktionen
// (Service Bus-triggad) landar här efter att ha läst kön och transformerat
// meddelandet. BillFlow behöver aldrig veta att Service Bus, eller ens
// MeterCore, existerar — den ser bara ett HTTP-anrop med data i det format
// den själv äger.
app.MapGet("/api/usage", (UsageStore store) => Results.Ok(store.GetAll()));

app.MapPost("/api/usage", (CreateUsageRecordRequest request, UsageStore store) =>
{
    var created = store.Add(request);
    return Results.Created($"/api/usage/{created.Id}", created);
});

app.Run();
