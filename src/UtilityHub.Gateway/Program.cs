using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// "Isolated worker" innebär att funktionskoden körs i sin egen .NET-process,
// separat från Azure Functions-värdens process. Det är skälet till att detta
// projekt har ett vanligt Program.cs och egen DI-container (samma modell som
// din Landskrona-funktion på .NET 10), till skillnad från äldre
// "in-process"-funktioner som delade process med värden.
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        services.AddHttpClient("BillFlow", client =>
        {
            var baseUrl = context.Configuration["BillFlowApi:BaseUrl"]
                ?? throw new InvalidOperationException("BillFlowApi:BaseUrl saknas i konfigurationen (local.settings.json eller App Setting i Azure).");
            client.BaseAddress = new Uri(baseUrl);
        });
    })
    .Build();

host.Run();
