dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator "-reports:TestResults/**/coverage.cobertura.xml" "-targetdir:CoverageReport" -reporttypes:Html
pause
# In order to successfully run coverage generator
#   1. Make this file executable by running command in terminal
#   chmod +x _PATH_TO_FILE_
#   2. Run this script by simply pasting _PATH_TO_FILE_ in terminal
# 
#   WARNING! Make sure that path to reportgenerator in PATH
#   Fix by:
#      nano ~/.zshrc
#      [PASTE THIS] export PATH="$PATH:~/.dotnet/tools"
#      Press CTRL + O and ENTER
#      Press CTRL + X
#      source ~/.zshrc
#      echo $PATH
#
#   WARNING! Make sure that you escaped any special characters
#   Fix by:
#   Use '\' - looks like \! \\ \' \" \$ etc.