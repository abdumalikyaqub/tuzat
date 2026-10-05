#Requires -Version 5.1
$ErrorActionPreference = 'Stop'
Set-Location $PSScriptRoot

dotnet test --configuration Release
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

dotnet publish src/Tuzat.App -c Release -r win-x64 --self-contained `
    -p:PublishSingleFile=true `
    -p:EnableCompressionInSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -o artifacts/publish
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$version = (dotnet msbuild src/Tuzat.App -getProperty:Version).Trim()

$iscc = (Get-Command iscc -ErrorAction SilentlyContinue).Source
if (-not $iscc) {
    $iscc = @(
        "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "$env:ProgramFiles\Inno Setup 6\ISCC.exe"
    ) | Where-Object { Test-Path $_ } | Select-Object -First 1
}
if (-not $iscc) {
    Write-Error 'Inno Setup 6 not found. Install it: winget install --id JRSoftware.InnoSetup --exact'
}

& $iscc "/DAppVersion=$version" installer/Tuzat.iss
exit $LASTEXITCODE
