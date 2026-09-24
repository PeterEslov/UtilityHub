# UtilityHub.Gateway

Platshållare för Fas 2–3 av arkitekturspecen:

- **Fas 2**: Azure Functions som reagerar på Event Grid-händelser (t.ex. nytt
  mätvärde → uppdatera förbrukningshistorik; avbrott registrerat → notifiera
  CustomerDesk)
- **Fas 3**: en Logic Apps Standard-workflow för "kund flyttar"-flödet, samt
  Durable Functions för saga-/kompensationslogik

Ingen kod här ännu — mini-API:erna (`src/*.Api`) är grunden och byggs och
körs fristående via `docker-compose`. Det här projektet fylls i när
händelsedrivna flöden och orkestrering ska läggas till, se roadmap-avsnittet
i arkitekturspecen.
