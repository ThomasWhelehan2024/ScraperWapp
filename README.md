# ScraperWapp

Open `ScraperWapp.sln`  with .NET 8.0 installed. Restore NuGet packages, 
then build and publish the project (or run `dotnet publish -c Release -r win-x64 --self-contained true`). 
Place the CSV file `land_registry_searches_4week.csv` in the published folder, then run `ScraperWapp.exe`. 
The app will open a browser at `https://localhost:5000`; if not, open the URL manually. 
