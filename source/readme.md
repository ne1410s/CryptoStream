## Crypt

``` powershell
# Restore tools
dotnet tool restore

# Run unit tests (multiple test projects, no threshold)
gci **/TestResults/ | ri -r; dotnet test -c Release -s .runsettings; dotnet reportgenerator -targetdir:coveragereport -reports:**/coverage.cobertura.xml -reporttypes:"html;jsonsummary"; start coveragereport/index.html;

# Run mutation tests and show report (per project)
$tp="Crypt.Tests"; gci **/StrykerOutput/ | ri -r; dotnet stryker -o -tp $tp -f "$tp/stryker-config.json"
```

## Original Project Setup
```powershell
# check dotnet versions
dotnet --list-sdks

# set dotnet version (remember to tweak pre-release, and newer versions)
dotnet new globaljson --sdk-version 6.0.400

# add a tool manifest
dotnet new tool-manifest

# add some tools
dotnet tool install dotnet-reportgenerator-globaltool
dotnet tool install dotnet-stryker
```