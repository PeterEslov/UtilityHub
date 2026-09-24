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

## Köra lokalt

Kräver Docker Desktop.

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

Swagger UI finns på `/swagger` för respektive tjänst.

## Repo-struktur

```
UtilityHub/
├── src/
│   ├── MeterCore.Api/          (mini-API, egen container)
│   ├── BillFlow.Api/           (mini-API, egen container)
│   ├── OutageWatch.Api/        (mini-API, egen container)
│   ├── CustomerDesk.Api/       (mini-API, egen container)
│   ├── UtilityHub.Gateway/     (Fas 2–3: Functions/Logic Apps, platshållare)
│   └── UtilityHub.DataSeeder/  (Fas 1: databas-seedning, platshållare)
├── infra/                      (Bicep, byggs ut fasvis)
├── docker-compose.yml
├── azure-pipelines.yml
└── UtilityHub.sln
```

## Status / roadmap

Fas 0 (grund) är på plats: fyra körbara mini-API:er med syntetisk data,
containeriserade, mono-repo, Azure DevOps-projekt `OresundskraftNorr`.

Nästa steg (Fas 1–4) finns beskrivna i arkitekturspecen: transform-and-forward
mellan MeterCore och BillFlow via Service Bus, händelsedrivna flöden med Event
Grid, orkestrering med Logic Apps/Durable Functions för "kund flyttar"-flödet,
och till sist APIM + Azure Container Apps i molnet.
