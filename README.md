# CryptoStream
## Overview
CryptoStream is a library that features some cryptographic functions and stream support.
This includes base implementation of a "BlockStream" and a "CryptoBlockStream" implemented with gcm.

## Notes
### Commands
```powershell
# Restore tools
dotnet tool restore

# Run unit tests
gci **/TestResults/ | ri -r; dotnet test -c Release -s .runsettings; dotnet reportgenerator -targetdir:coveragereport -reports:**/coverage.cobertura.xml -reporttypes:"html;jsonsummary"; start coveragereport/index.html;

# Run mutation tests
gci **/StrykerOutput/ | ri -r; dotnet stryker -o;

# Pack and publish a pre-release to a local feed
$suffix="alpha001"; dotnet pack -c Release -o nu --version-suffix $suffix; dotnet nuget push "nu\*.*$suffix.nupkg" --source localdev; gci nu/ | ri -r; rmdir nu;
```
