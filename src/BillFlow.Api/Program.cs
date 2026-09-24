using BillFlow.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<InvoiceStore>();
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

app.Run();
