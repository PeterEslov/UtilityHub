// UtilityHub.DataSeeder
//
// I dagens läge (Fas 0) seedar varje mini-API sin egen syntetiska data
// in-process via Bogus (se DataStore.cs i respektive projekt) — det räcker
// för att köra och demonstrera flödena lokalt utan någon databas alls.
//
// Det här projektet är reserverat för Fas 1, när mini-API:erna får riktiga
// databaser (Azure SQL per system, se arkitekturspecen). Då blir jobbet
// här att:
//   1. Läsa/generera samma typ av syntetisk data (mätvärden, fakturor,
//      avbrott, kunder/ärenden) med Bogus
//   2. Skriva den till respektive databas vid uppstart av en ny miljö
//   3. Köras en gång per miljö (lokalt via docker-compose, eller mot en
//      riktig Azure SQL-instans) snarare än vid varje apprestart
//
// Se README.md i repo-roten och UtilityHub-arkitekturspecen för detaljer.

Console.WriteLine("UtilityHub.DataSeeder — platshållare för Fas 1 (databas-seedning).");
Console.WriteLine("Just nu seedar varje mini-API sin egen data in-process. Se kommentaren i Program.cs.");
