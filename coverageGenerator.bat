dotnet test --collect:"XPlat Code Coverage" --results-directory ./Tests/TestResults
#dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator "-reports:Tests/TestResults/**/coverage.cobertura.xml" "-targetdir:Tests/CoverageReport" -reporttypes:Html
pause
# Before execution make sure you installed report generator
#   Fix by uncomment dotnet tool install command