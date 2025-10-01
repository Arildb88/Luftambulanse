										Luftambulanse
- En ASP.NET Core MVC-applikasjon som demonstrerer grunnlegende web-funksjonalitet for luftambulasnse-domene (sider som Home/Index, Privacy og Error, samtidig struktur
   for å legge til modeller, kontrollere og views. Applikasjonen er å gi brukere en enkel og oversiktlig måte å register, og håndtere informasjon om lufthindringer. 
   Systemet gjør det mulig å samle inn opplysninger¨, vise dem i en strukturtrert form og sikre at de kan oppdatere på en ryddig måte.

	INNHOLD

- 	Teknologi

-       Krav 

-       Komme i gang 

-       Kjøring 

-       Prosjektstruktur 

-       Konfigurasjon 

-       Logging og Feil 

-       Testing 

-       Lisens

	Teknologi 	

-       .NET 8 (ASP.NET)

-       Razor Views

-       Innebygd Dependency Injection og Logging

-	MariaD/MySQL

- 	

	Krav

-       .NET SDK

- 	VS Code 

-       MariaDB/MySQL

 
	Kom i gang 	
# Klon repo
git clone https://github.com/Arildb88/Luftambulanse.git
cd Luftambulanse

# Gjenopprett pakker
dotnet restore

# Bygg
dotnet build

	Kjøring 
# Start utviklingsserver
dotnet run

# eller med hot reload
dotnet watch

	Prosjektstruktur

Luftambulanse/
├─ Controllers/
│  └─ HomeController.cs         # Returnerer Index, Privacy og Error views
├─ Models/
│  └─ ErrorViewModel.cs         # Brukes av feilsiden
├─ Views/
│  ├─ Home/
│  │  ├─ Index.cshtml
│  │  └─ Privacy.cshtml
│  └─ Shared/
│     ├─ _Layout.cshtml
│     └─ Error.cshtml
├─ wwwroot/                     # CSS/JS/bilder
├─ appsettings.json             # Konfig (logging, evt. connection strings)
└─ Program.cs / Startup.cs      # App bootstrap og routing

	Konfigurasjon 

All konfig ligger i appsettings.json og miljøspesifikke filer (appsettings.Development.json).
Eksempel:
{
  "Logging": { "LogLevel": { "Default": "Information", "Microsoft.AspNetCore": "Warning" } },
  "AllowedHosts": "*"
  // "ConnectionStrings": { "Default": "Server=...;Database=...;User Id=...;Password=...;" }
}

- Dermed det legges til database, opprett en "ConnectionStrings:Defualt" og hent den i Program.cs når dere registerer DbContext.

	Logging og Feil 
- Logging leveres av ILogger<T> via dependency injection (se HomeController).

- Feilsiden (/Home/Error) viser en RequestId fra Activity.Current?.Id ?? HttpContext.TraceIdentifier.

- ResponseCache på Error() forhindrer caching av feilsiden.

	Testing 
- Manuelle tester: naviger til /Home/Index og /Home/Privacy.

- Legg gjerne til enhetstester med xUnit:
	
dotnet new xunit -n Luftambulanse.Tests


Koble testprosjektet mot webprosjektet og kjør dotnet test.


	

										Lisens
									      MIT License

									The MIT License (MIT)

								Copyright (c) 2015 Chris Kibble

	Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), 
	to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
	and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

			The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
	WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
