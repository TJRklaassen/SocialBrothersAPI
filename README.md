# SocialBrothersAPI
## Instructies
1. Clone de repository.
2. Open een commandline interface in de root folder van het project.
3. Voer command `dotnet run` uit als .NET correct is geïnstalleerd.
4. De swagger documentatie pagina opent als het goed is automatisch.
5. Test de verschillende API endpoints uit.

## Goede en slechte punten
### Goed ###
De code voor het filteren en sorteren van Addresses is beknopt en volledig dynamisch en het maakt dus ook niet uit of je velden aan Address verwijderd of toevoegt.
### Slecht ###
De CalculateDistance methode is niet volledig accuraat.
De afstand in km komt wel in de buurt maar zit er meestal net iets onder.
Dit zou ik graag nog wel willen oplossen, maar in principe is de logica en mijn aanpak van het proces duidelijk.
