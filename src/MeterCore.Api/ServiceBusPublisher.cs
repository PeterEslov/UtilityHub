using System.Text.Json;
using Azure.Messaging.ServiceBus;

namespace MeterCore.Api;

/// <summary>
/// Thin wrapper around a Service Bus sender for the "meter-readings" queue.
///
/// Why a queue and not a direct HTTP call to BillFlow? Two systems talking
/// straight to each other over HTTP are temporally coupled: BillFlow has to
/// be up, reachable, and fast enough right now, or the call fails. A queue
/// breaks that coupling — MeterCore's job ends the moment the message is
/// safely in the queue. If BillFlow (or the Function reading the queue) is
/// down for five minutes, nothing is lost; it just catches up when it's back.
/// That's "guaranteed delivery" in practice, and it's why Service Bus (not
/// just an in-memory event) was the pattern chosen in the architecture spec.
/// </summary>
public class ServiceBusPublisher : IAsyncDisposable
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusSender _sender;

    public ServiceBusPublisher(IConfiguration configuration)
    {
        var connectionString = configuration["ServiceBus:ConnectionString"]
            ?? throw new InvalidOperationException("ServiceBus:ConnectionString saknas i konfigurationen.");
        var queueName = configuration["ServiceBus:MeterReadingsQueue"] ?? "meter-readings";

        _client = new ServiceBusClient(connectionString);
        _sender = _client.CreateSender(queueName);
    }

    public async Task PublishMeterReadingRecordedAsync(MeterReadingRecordedMessage message, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(message);
        var serviceBusMessage = new ServiceBusMessage(json)
        {
            ContentType = "application/json",
            Subject = nameof(MeterReadingRecordedMessage),
            // CorrelationId gör att en och samma händelse går att spåra genom
            // hela kedjan i Application Insights, även när den passerar en kö.
            CorrelationId = Guid.NewGuid().ToString()
        };

        await _sender.SendMessageAsync(serviceBusMessage, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
        await _client.DisposeAsync();
    }
}
