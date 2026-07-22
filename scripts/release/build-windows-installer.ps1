param(
    [string] $Version = "1.0.0",
    [string] $Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$artifactRoot = Join-Path $repoRoot "artifacts"
$publishDir = Join-Path $artifactRoot "publish\RobotStudio-$Version-$Runtime"
$releaseDir = Join-Path $artifactRoot "release"
$installerWorkDir = Join-Path $artifactRoot "installer-work"
$payloadZip = Join-Path $installerWorkDir "RobotStudio-$Version-$Runtime.zip"
$installScript = Join-Path $installerWorkDir "install.ps1"
$sedFile = Join-Path $installerWorkDir "RobotStudio-$Version-$Runtime.sed"
$installerPath = Join-Path $releaseDir "RobotStudio-$Version-$Runtime-setup.exe"

New-Item -ItemType Directory -Force -Path $publishDir, $releaseDir, $installerWorkDir | Out-Null

Get-ChildItem -LiteralPath $releaseDir -Filter "~RobotStudio-$Version-$Runtime-setup.*" -ErrorAction SilentlyContinue |
    Remove-Item -Force

dotnet publish (Join-Path $repoRoot "src\RobotStudio.Desktop\RobotStudio.Desktop.csproj") `
    --configuration Release `
    --runtime $Runtime `
    --self-contained true `
    --output $publishDir `
    -p:PublishSingleFile=false `
    -p:PublishReadyToRun=false

if (Test-Path $payloadZip) {
    Remove-Item -LiteralPath $payloadZip -Force
}

Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $payloadZip -Force

@"
`$ErrorActionPreference = "Stop"

`$appName = "RobotStudio"
`$installRoot = Join-Path `$env:LOCALAPPDATA `$appName
`$installDir = Join-Path `$installRoot "app"
`$payload = Join-Path `$PSScriptRoot "RobotStudio-$Version-$Runtime.zip"
`$exePath = Join-Path `$installDir "RobotStudio.Desktop.exe"

if (Test-Path `$installDir) {
    Remove-Item -LiteralPath `$installDir -Recurse -Force
}

New-Item -ItemType Directory -Force -Path `$installDir | Out-Null
Expand-Archive -LiteralPath `$payload -DestinationPath `$installDir -Force

`$shell = New-Object -ComObject WScript.Shell
`$desktopShortcut = Join-Path ([Environment]::GetFolderPath("DesktopDirectory")) "RobotStudio.lnk"
`$startMenuDir = Join-Path ([Environment]::GetFolderPath("Programs")) "RobotStudio"
`$startMenuShortcut = Join-Path `$startMenuDir "RobotStudio.lnk"

New-Item -ItemType Directory -Force -Path `$startMenuDir | Out-Null

foreach (`$shortcutPath in @(`$desktopShortcut, `$startMenuShortcut)) {
    `$shortcut = `$shell.CreateShortcut(`$shortcutPath)
    `$shortcut.TargetPath = `$exePath
    `$shortcut.WorkingDirectory = `$installDir
    `$shortcut.IconLocation = "`$exePath,0"
    `$shortcut.Description = "RobotStudio"
    `$shortcut.Save()
}

Start-Process -FilePath `$exePath
"@ | Set-Content -LiteralPath $installScript -Encoding UTF8

$escapedWorkDir = $installerWorkDir.TrimEnd("\")
$escapedInstallerPath = $installerPath

@"
[Version]
Class=IEXPRESS
SEDVersion=3

[Options]
PackagePurpose=InstallApp
ShowInstallProgramWindow=0
HideExtractAnimation=1
UseLongFileName=1
InsideCompressed=1
CAB_FixedSize=0
CAB_ResvCodeSigning=0
RebootMode=N
InstallPrompt=
DisplayLicense=
FinishMessage=RobotStudio $Version has been installed.
TargetName=$escapedInstallerPath
FriendlyName=RobotStudio $Version Setup
AppLaunched=powershell.exe -NoProfile -ExecutionPolicy Bypass -File install.ps1
PostInstallCmd=<None>
AdminQuietInstCmd=
UserQuietInstCmd=
SourceFiles=SourceFiles

[SourceFiles]
SourceFiles0=$escapedWorkDir

[SourceFiles0]
%FILE0%=
%FILE1%=

[Strings]
FILE0="RobotStudio-$Version-$Runtime.zip"
FILE1="install.ps1"
"@ | Set-Content -LiteralPath $sedFile -Encoding ASCII

$iexpress = Start-Process -FilePath "iexpress.exe" -ArgumentList @("/N", $sedFile) -Wait -PassThru

if ($iexpress.ExitCode -ne 0) {
    throw "IExpress failed with exit code $($iexpress.ExitCode)."
}

if (-not (Test-Path $installerPath)) {
    throw "Installer was not created at $installerPath."
}

Start-Sleep -Seconds 1

Get-ChildItem -LiteralPath $releaseDir -Filter "~RobotStudio-$Version-$Runtime-setup.*" -ErrorAction SilentlyContinue |
    Remove-Item -Force

Write-Host "Windows installer created:"
Write-Host $installerPath
