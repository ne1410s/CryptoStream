## Crypt

``` powershell
# Restore tools
dotnet tool restore

# Run unit tests and show coverage report
gci **/TestResults/ | ri -r; dotnet build -c Release -o bin; dotnet coverlet bin/Crypt.Tests.dll -t dotnet -a "test bin/Crypt.Tests.dll -c Release --no-build" --threshold 100 -f cobertura -o TestResults/coverage; dotnet reportgenerator -targetdir:coveragereport -reports:**/coverage.cobertura.xml -reporttypes:"html"; start coveragereport/index.html;

# Run mutation tests and show report
if (Test-Path StrykerOutput) { rm -r StrykerOutput }; dotnet stryker -o
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
dotnet tool install coverlet.console
dotnet tool install dotnet-reportgenerator-globaltool
dotnet tool install dotnet-stryker
```