$modulePath = Join-Path $PSScriptRoot "Home-Assistant"

# Paths
$projectDir = Join-Path $PSScriptRoot "../src/HomeAutomation"
$projectPath = Join-Path $projectDir "HomeAutomation.csproj"
$appSettings = Join-Path $projectDir "appsettings.json"
$appSettingsDev = Join-Path $projectDir "appsettings.Development.json"

$settings = (Get-Content $appSettings | ConvertFrom-Json)

#CHANGE ME
$slug = 'c6a2317c_netdaemon5' # the slug can be found in the url of the browser when navigating to the NetDaemon addon

$version = $slug.Split('_')[-1] # adapt if you are not using the default foldername for the addon
$json = '{"addon": "' + $slug + '"}'
$ip = $settings.HomeAssistant.Host
$port = $settings.HomeAssistant.Port

$token = (Get-Content $appSettingsDev | ConvertFrom-Json).HomeAssistant.Token

# Point to the HA PowerShell Module

Unblock-File "$modulePath\Home-Assistant.psd1"
Unblock-File "$modulePath\Home-Assistant.psm1"
Import-Module "$modulePath\Home-Assistant.psd1"

New-HomeAssistantSession -ip  $ip -port $port -token $token

Remove-Item -Recurse -Force \\$ip\config\$version\*
dotnet publish -c Release $projectPath -o \\$ip\config\$version\

Invoke-HomeAssistantService -service hassio.addon_restart -json $json