param([switch]$Package)
$ErrorActionPreference='Stop'
$framework=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319'
$dist=Join-Path $PSScriptRoot $(if($Package){'dist'}else{'preview'})
$null=New-Item -ItemType Directory -Force $dist
$compiler=Join-Path $framework 'csc.exe'
$refs=@('System.dll','System.Core.dll','System.Web.Extensions.dll','System.Windows.Forms.dll','System.Drawing.dll','WPF/PresentationFramework.dll','WPF/PresentationCore.dll','WPF/WindowsBase.dll','System.Xaml.dll') | ForEach-Object {'/reference:'+(Join-Path $framework $_)}
& $compiler /nologo /target:winexe /platform:anycpu /optimize+ ("/out:"+(Join-Path $dist 'CodexQuotaWidget.exe')) ("/win32icon:"+(Join-Path $PSScriptRoot 'assets/app.ico')) ("/resource:"+(Join-Path $PSScriptRoot 'assets/app.ico')+',App.ico') ("/win32manifest:"+(Join-Path $PSScriptRoot 'native/app.manifest')) ("/resource:"+(Join-Path $PSScriptRoot 'native/Widget.xaml')+',Widget.xaml') @refs (Join-Path $PSScriptRoot 'native/Program.cs') (Join-Path $PSScriptRoot 'native/Glass.cs') (Join-Path $PSScriptRoot 'native/DesktopMaterial.cs') (Join-Path $PSScriptRoot 'native/ThemeManager.cs')
if($LASTEXITCODE -ne 0){throw 'Build failed'}
if($Package){
 Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'DISTRIBUTION.md') -Destination (Join-Path $dist 'README.md') -Force
 Compress-Archive -LiteralPath (Join-Path $dist 'CodexQuotaWidget.exe'),(Join-Path $dist 'README.md') -DestinationPath (Join-Path $dist 'CodexQuotaWidget-Windows.zip') -Force
}
Get-FileHash (Join-Path $dist 'CodexQuotaWidget.exe') -Algorithm SHA256
