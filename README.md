# SocialBrothersAPI
## Instructies
### Opstarten ###
1. Clone de repository.
2. Open een commandline interface in de root folder van het project.
3. Voer command `dotnet run` uit als .NET correct is geïnstalleerd.
4. De swagger documentatie pagina opent als het goed is automatisch.
### Gebruiken endpoints ###
- Voor de filter endpoint gebruik je `/api/address/filter/{value}`. Hierbij kan value alles zijn en er wordt dan in ieder field van alle adressen gezocht op deze value. Gebruik bijvoorbeeld 'dreef'.
- Voor de sort endpoint gebruik je `/api/address/sort/{direction}/{field}`. Hierbij moet direction 'ascending' of 'descending' zijn en field moet een bestaande field uit de Address model zijn. Gebruik bijvoorbeeld 'ascending' en 'street'.
- voor de distance endpoint gebruik je `/api/address/distance/{id1}/{id2}`. Hierbij is id1 en id2 allebei de id van een bestaande Address in de database. Gebruik bijvoorbeeld 1 en 9.
- Alle standaard CRUD endpoints spreken voor zich.

## Goede en slechte punten
### Goed ###
De code voor het filteren en sorteren van Addresses is beknopt en volledig dynamisch en het maakt dus ook niet uit of je velden aan Address verwijderd of toevoegt.
### Slecht ###
De CalculateDistance methode is niet volledig accuraat.
De afstand in km komt wel in de buurt maar zit er meestal net iets onder.
Dit zou ik graag nog wel willen oplossen, maar in principe is de logica en mijn aanpak van het proces duidelijk.
