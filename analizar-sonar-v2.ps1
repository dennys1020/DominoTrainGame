$ErrorActionPreference = "Stop"

$proyecto = "dennys1020_DominoTrainGame"
$organizacion = "dennys1020"
$solucionServidor = "Server\Server.slnx"
$solucionCliente = "Client\Client.slnx"
$pruebas = @(
    "Application.Tests.Unit",
    "Persistence.Tests.Unit",
    "Service.Tests.Unit"
)
$exclusiones = "**/Connected Services/**,**/*.Designer.cs,**/Properties/AssemblyInfo.cs"

if (-not $env:SONAR_TOKEN) {
    throw "Falta la variable de entorno SONAR_TOKEN"
}

$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
if (-not (Test-Path $vswhere)) {
    throw "No se encontro vswhere.exe; verifica que Visual Studio este instalado"
}

$msbuild = & $vswhere -latest -prerelease -products * -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1
if (-not $msbuild) {
    throw "No se encontro MSBuild.exe"
}

if (-not (Get-Command dotnet-coverage -ErrorAction SilentlyContinue)) {
    throw "Falta dotnet-coverage; instalalo con: dotnet tool install --global dotnet-coverage"
}

Write-Host "MSBuild: $msbuild"

function Invoke-Paso {
    param([string]$Descripcion, [scriptblock]$Comando)
    Write-Host "==> $Descripcion" -ForegroundColor Cyan
    & $Comando
    if ($LASTEXITCODE -ne 0) {
        throw "Fallo el paso '$Descripcion' (codigo $LASTEXITCODE)"
    }
}

Invoke-Paso "SonarScanner begin" {
    dotnet sonarscanner begin /k:$proyecto /o:$organizacion `
        /d:sonar.host.url="https://sonarcloud.io" `
        /d:sonar.token="$env:SONAR_TOKEN" `
        /d:sonar.cs.vscoveragexml.reportsPaths="coverage-*.xml" `
        /d:sonar.exclusions="$exclusiones"
}

Invoke-Paso "Compilar servidor" {
    & $msbuild $solucionServidor /restore /t:Rebuild /p:Configuration=Debug /p:RestorePackagesConfig=true /nodeReuse:false
}

Invoke-Paso "Compilar cliente" {
    & $msbuild $solucionCliente /restore /t:Rebuild /p:Configuration=Debug /p:RestorePackagesConfig=true /nodeReuse:false
}

foreach ($prueba in $pruebas) {
    Invoke-Paso "Pruebas $prueba con cobertura" {
        dotnet-coverage collect "dotnet test tests\$prueba\$prueba.csproj --no-build -c Debug" -f xml -o "coverage-$prueba.xml"
    }
}

Invoke-Paso "SonarScanner end" {
    dotnet sonarscanner end /d:sonar.token="$env:SONAR_TOKEN"
}
