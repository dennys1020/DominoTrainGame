$proyecto = "dennys1020_DominoTrainGame"
$organizacion = "dennys1020"
$solucionServidor = "Server\Server.slnx"
$solucionCliente = "Client\Client.slnx"
$pruebas = "tests\Application.Tests.Unit\bin\Debug\net48\Application.Tests.Unit.dll"

dotnet sonarscanner begin /k:$proyecto /o:$organizacion `
  /d:sonar.host.url="https://sonarcloud.io" `
  /d:sonar.token="$env:SONAR_TOKEN" `
  /d:sonar.cs.vscoveragexml.reportsPaths="coverage.xml"

msbuild $solucionServidor /restore /t:Rebuild /p:Configuration=Debug /p:RestorePackagesConfig=true
msbuild $solucionCliente /restore /t:Rebuild /p:Configuration=Debug /p:RestorePackagesConfig=true

dotnet-coverage collect "vstest.console.exe $pruebas" -f xml -o "coverage.xml"

dotnet sonarscanner end /d:sonar.token="$env:SONAR_TOKEN"
