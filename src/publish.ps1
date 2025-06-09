$settings = (Get-Content appsettings.json | ConvertFrom-Json)

#CHANGE ME
$slug = 'c6a2317c_netdaemon5' # the slug can be found in the url of the browser when navigating to the NetDaemon addon

$version = $slug.Split('_')[-1] # adapt if you are not using the default foldername for the addon
$json = '{"addon": "' + $slug + '"}'
$ip = $settings.HomeAssistant.Host
$port = $settings.HomeAssistant.Port

$token = (Get-Content appsettings.Development.json | ConvertFrom-Json).HomeAssistant.Token

# Point to the HA PowerShell Module
Unblock-File .\Home-Assistant\Home-Assistant.psd1
Unblock-File .\Home-Assistant\Home-Assistant.psm1
Import-Module .\Home-Assistant

New-HomeAssistantSession -ip  $ip -port $port -token $token

Invoke-HomeAssistantService -service hassio.addon_stop -json $json

Remove-Item -Recurse -Force \\$ip\config\$version\*
dotnet publish -c Release HomeAutomation.csproj -o \\$ip\config\$version

Invoke-HomeAssistantService -service hassio.addon_start -json $json