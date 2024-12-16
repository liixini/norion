# Norion Bank biltull Programmeringstest

Programmet är ett minimal API skrivet i .NET 8 som beräknar biltullar.
Idén var att bygga ett program runt de kodfiler jag fick för att kunna testa programmet, köra tester m.m. och ett API passar det endamålet väldigt bra, speciellt med möjligheten att byta inskickade json-objekt enkelt.

All data som tidigare var hårdkodad är utbruten till databas (MSSQL) där programmet läser upp datan via repos, med undantag för högsta dagsbeloppet samt hur länge man kan åka på samma avgift som läses från konfigurationen - ingen egentlig orsak för det förutom att konfigurationen inte användes för någonting.

Syftet med utbrytningen är så att man enkelt kan administrera de regler som sker kring helgdatum m.m. då tidigare implementation krävde att mjukvaran uppdateras om t.ex. en dag ska vara avgiftsfri.

Utöver detta så är lite middleware uppsatt, såsom loggning, secrets (connectionsträng till min lokala db) och exception-hantering vid databasanslutningen. Databasanslutningen sker genom ORM:en Dapper.

Jag gjorde antagandet att originalkoden inte behövde respekteras, t.ex. metodsignaturer, och har således modifierat och döpt om det mesta för att bättre passa faktumet att programmet går igång på ett JSON-objekt som skickas till endpointen.

Enhetstester är endast skrivna för TollFreeService med en coverage på 82% och det finns inga integrations- eller end-to-endtester. API:et är heller inte säkrat och är helt öppet för anrop.

Förbättringsförslag för programmet idag är t.ex. att versionera datan i databasen så att flera olika regelsätt kan användas oberoende av faktiska datum, t.ex. om tullavgift räknas ut på ett datum efter reglerna är ändrade med själva datumen är före.

Nackdelen med att ha en datadriven applikation är att varje undantag för reglerna behöver ett motsvarande datum i databasen t.ex. varje helg, till skillnad från den aningen mer praktiska "om lördag eller söndag så...", men oavsett så ger detta oss en mycket mer precis lösning som kan modiferas efter behov utan en mjukvaruuppdatering.