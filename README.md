# UtilityHub — Öresundskraft Norr

Portfolio-projekt som visar enterprise-integrationsmönster på Azure: pub/sub,
garanterad meddelandeleverans, orkestrering vs koreografi, saga-mönster,
API-gateway och transform-and-forward. Fullständig arkitekturspec (scenario,
mönster, roadmap) finns i det delade UtilityHub-dokumentet.

Ingen riktig produktionsdata används någonstans i projektet — alla fyra
källsystem seedas med syntetisk data via [Bogus](https://github.com/bchavez/Bogus).

## Systemlandskap (simulerat)

| System | Roll | Port (lokalt) |
|---|---|---|
| MeterCore.Api | Mätvärdesinsamling | 5101 |
| BillFlow.Api | Fakturering | 5102 |
| OutageWatch.Api | Avbrottshantering | 5103 |
| CustomerDesk.Api | Kundtjänst-CRM | 5104 |

Varje API är en egen container, körs oberoende, och seedar sin egen
in-memory-data vid uppstart (se `DataStore.cs` i respektive projekt).

## Fas 1: transform-and-forward (klart)

`MeterCore.Api` publicerar ett `MeterReadingRecordedMessage` till Service
Bus-kön `meter-readings` varje gång ett nytt mätvärde skapas. Funktionsappen
`UtilityHub.Gateway` lyssnar på kön, transformerar meddelandet till det
format `BillFlow.Api` förväntar sig, och POST:ar det till `/api/usage`.

Lokalt körs Service Bus via Microsofts officiella emulator (i
docker-compose); ingen riktig Azure-resurs behövs för att testa flödet.

## Köra lokalt

Kräver Docker Desktop, samt [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local)
och Azurite för att köra Gateway-funktionen (se `src/UtilityHub.Gateway/README.md`).

```bash
docker compose up --build
```

Sedan, t.ex.:

```bash
curl http://localhost:5101/api/meterreadings
curl http://localhost:5102/api/invoices
curl http://localhost:5103/api/outages/active
curl http://localhost:5104/api/customers
```

Testa hela transform-and-forward-kedjan (kräver att Gateway-funktionen också
körs, se dess README):

```bash
curl -X POST http://localhost:5101/api/meterreadings \
  -H "Content-Type: application/json" \
  -d '{"meterId":"MTR-00001","customerId":"CUST-00001","consumptionKwh":4.2}'

# vänta någon sekund, kolla sedan att den dök upp i BillFlow:
curl http://localhost:5102/api/usage
```

Swagger UI finns på `/swagger` för respektive API.

## Repo-struktur

```
UtilityHub/
├── src/
│   ├── MeterCore.Api/          (mini-API, egen container)
│   ├── BillFlow.Api/           (mini-API, egen container)
│   ├── OutageWatch.Api/        (mini-API, egen container)
│   ├── CustomerDesk.Api/       (mini-API, egen container)
│   ├── UtilityHub.Gateway/     (Fas 1: Service Bus → transform → BillFlow)
│   └── UtilityHub.DataSeeder/  (Fas 1+: databas-seedning, platshållare)
├── infra/                      (Bicep + Service Bus-emulatorns config)
├── docker-compose.yml
├── azure-pipelines.yml
└── UtilityHub.sln
```

## Status / roadmap

- **Fas 0** (grund): klar — fyra körbara mini-API:er, containeriserade, mono-repo
- **Fas 1** (transform-and-forward): klar — se ovan
- **Fas 2** (pub/sub & koreografi): Event Grid för avbrottshändelser — näst på tur
- **Fas 3** (orkestrering & saga): Logic Apps/Durable Functions för "kund flyttar"
- **Fas 4** (gateway & polish): APIM + Azure Container Apps i molnet

Fullständig beskrivning av varje fas finns i arkitekturspecen.
