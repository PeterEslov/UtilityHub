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
    var validationErrors = request.Validate();
    if (validationErrors.Count > 0)
    {
        // Om Gateway-funktionen skickar ett ogiltigt värde hit får den 400
        // tillbaka, vilket får dess EnsureSuccessStatusCode() att kasta ett
        // exception — och DET gör i sin tur att Service Bus-triggern INTE
        // committar meddelandet. Meddelandet läggs tillbaka i kön och
        // försöks igen. Samma återförsöksmekanism som beskrivs i
        // ForwardMeterReadingToBillingFunction.cs, nu synlig från andra hållet.
        return Results.ValidationProblem(validationErrors);
    }

    var created = store.Add(request);
    return Results.Created($"/api/usage/{created.Id}", created);
});

app.Run();
