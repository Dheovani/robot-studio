param(
    [string] $Version = "1.0.0",
    [string] $Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$artifactRoot = Join-Path $repoRoot "artifacts"
$publishDir = Join-Path $artifactRoot "publish\RobotStudio-$Version-cli-$Runtime"
$releaseDir = Join-Path $artifactRoot "release"
$archivePath = Join-Path $releaseDir "RobotStudio-$Version-cli-$Runtime.zip"
$checksumPath = "$archivePath.sha256"

New-Item -ItemType Directory -Force -Path $publishDir, $releaseDir | Out-Null

dotnet publish (Join-Path $repoRoot "src\RobotStudio.Cli\RobotStudio.Cli.csproj") `
    --configuration Release `
    --runtime $Runtime `
    --self-contained true `
    --output $publishDir `
    -p:PublishSingleFile=false `
    -p:PublishReadyToRun=false

if (Test-Path $archivePath) {
    Remove-Item -LiteralPath $archivePath -Force
}

if (Test-Path $checksumPath) {
    Remove-Item -LiteralPath $checksumPath -Force
}

Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $archivePath -Force

$checksum = Get-FileHash -LiteralPath $archivePath -Algorithm SHA256
"$($checksum.Hash)  $(Split-Path $archivePath -Leaf)" | Set-Content -LiteralPath $checksumPath -Encoding ASCII

Write-Host "CLI artifact created:"
Write-Host $archivePath
Write-Host "SHA256 checksum created:"
Write-Host $checksumPath
