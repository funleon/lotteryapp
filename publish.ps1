$ErrorActionPreference = "Stop"

# Define paths
$projectPath = ".\lotteryapp.csproj"
$publishDir = ".\bin\Release\net8.0-windows10.0.19041.0\win-x64\publish"
$distDir = ".\Dist"
$zipFile = ".\LotteryApp_Portable.zip"

Write-Host "Cleaning previous builds..."
if (Test-Path $distDir) { Remove-Item -Recurse -Force $distDir }
if (Test-Path $zipFile) { Remove-Item -Force $zipFile }
dotnet clean $projectPath -c Release

Write-Host "Publishing application..."
# -r win-x64: Target 64-bit Windows
# --self-contained: Include .NET Runtime
# -p:WindowsPackageType=None: Not an MSIX package
# -p:WindowsAppSDKSelfContained=true: Include WinAppSDK
# -p:PublishSingleFile=true: Merge into single file (mostly)
dotnet publish $projectPath -c Release -r win-x64 --self-contained -p:WindowsPackageType=None -p:WindowsAppSDKSelfContained=true

Write-Host "Organizing output..."
New-Item -ItemType Directory -Force -Path $distDir | Out-Null

# Copy publish output to Dist
Copy-Item "$publishDir\*" -Destination $distDir -Recurse

# Copy necessary data files if they exist (though the app should create them)
# If you have default templates, copy them here. For now, we assume clean slate or self-generation.

Write-Host "Creating zip package..."
Compress-Archive -Path "$distDir\*" -DestinationPath $zipFile

Write-Host "Done! Package created at $zipFile"
