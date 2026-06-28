dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator "-reports:TestResults/**/coverage.cobertura.xml" "-targetdir:CoverageReport" -reporttypes:Html
pause