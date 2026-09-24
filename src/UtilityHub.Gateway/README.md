# UtilityHub.Gateway

Azure Functions-app (isolated worker, .NET 8) som utgör "forward"-steget i
Fas 1:s transform-and-forward-pipeline mellan MeterCore och BillFlow.

**Vad den gör:** lyssnar på Service Bus-kön `meter-readings` (som
`MeterCore.Api` publicerar till), transformerar meddelandet till det format
`BillFlow.Api` förväntar sig, och POST:ar det till `/api/usage`.

## Köra lokalt

Kräver [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local)
och Azurite (för funktionshostens eget lagringsbehov — `npm install -g azurite`
eller VS Code-tillägget).

1. Kopiera `local.settings.json.example` till `local.settings.json` (den
   filen är avsiktligt gitignorad eftersom den brukar innehålla lokala
   hemligheter, se repo-rotens `.gitignore`)
2. Starta de fyra mini-API:erna + Service Bus-emulatorn: `docker compose up`
   i repo-roten
3. Starta Azurite i en egen terminal: `azurite`
4. Starta funktionen: `func start` i den här mappen

Posta ett nytt mätvärde mot MeterCore (`POST http://localhost:5101/api/meterreadings`)
och se att det dyker upp i BillFlow (`GET http://localhost:5102/api/usage`)
en liten stund senare — det är hela transform-and-forward-kedjan i praktiken.

**Nästa fas (Fas 2)** bygger vidare på samma projekt med en Event
Grid-triggad funktion för avbrottshändelser.
