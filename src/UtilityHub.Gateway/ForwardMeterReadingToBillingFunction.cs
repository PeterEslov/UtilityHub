using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace UtilityHub.Gateway;

/// <summary>
/// The "forward" half of MeterCore's transform-and-forward pipeline.
///
/// [ServiceBusTrigger] means the Functions host itself listens to the queue
/// and invokes Run() for every message — you never write a polling loop.
/// "Connection = ServiceBusConnection" is a convention, not a literal
/// connection string: the runtime looks for a config value/app setting
/// named "ServiceBusConnection" (in local.settings.json locally, or an App
/// Setting in Azure) and uses whatever connection string is there.
/// </summary>
public class ForwardMeterReadingToBillingFunction
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ForwardMeterReadingToBillingFunction> _logger;

    public ForwardMeterReadingToBillingFunction(IHttpClientFactory httpClientFactory, ILogger<ForwardMeterReadingToBillingFunction> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [Function("ForwardMeterReadingToBilling")]
    public async Task Run(
        [ServiceBusTrigger("meter-readings", Connection = "ServiceBusConnection")] string message)
    {
        var reading = JsonSerializer.Deserialize<MeterReadingRecordedMessage>(message)
            ?? throw new InvalidOperationException("Kunde inte tolka meddelandet som MeterReadingRecordedMessage — kontrollera att MeterCore.Api skickar samma JSON-form.");

        _logger.LogInformation(
            "Vidarebefordrar {ConsumptionKwh} kWh för mätare {MeterId} (kund {CustomerId}) till BillFlow",
            reading.ConsumptionKwh, reading.MeterId, reading.CustomerId);

        // Själva transformationen: MeterCores meddelandeform blir BillFlows
        // UsageRecord-form. Just nu är mappningen nästan 1:1, men det är här
        // den typen av logik skulle växa — t.ex. att räkna om enheter, slå
        // upp kundens avtalstyp, eller filtrera bort estimerade avläsningar
        // innan de får påverka en faktura.
        var usageRequest = new
        {
            CustomerId = reading.CustomerId,
            SourceMeterId = reading.MeterId,
            ConsumptionKwh = reading.ConsumptionKwh,
            RecordedAt = reading.Timestamp
        };

        var client = _httpClientFactory.CreateClient("BillFlow");
        var response = await client.PostAsJsonAsync("/api/usage", usageRequest);

        // Ett exception härifrån gör att Service Bus INTE räknar meddelandet
        // som klart — det läggs tillbaka i kön och triggern körs igen (upp
        // till MaxDeliveryCount gånger, se infra/servicebus-config.json), och
        // hamnar till slut i dead-letter-kön om BillFlow förblir nere. Det är
        // just den återförsöksmekaniken — inte något Claude eller du skriver
        // för hand — som gör kön mer robust än ett direkt HTTP-anrop.
        response.EnsureSuccessStatusCode();
    }
}
