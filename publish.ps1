$settings = (Get-Content src/appsettings.json | ConvertFrom-Json)

#CHANGE ME
$slug = 'c6a2317c_netdaemon5' # the slug can be found in the url of the browser when navigating to the NetDaemon addon

$version = $slug.Split('_')[-1] # adapt if you are not using the default foldername for the addon
$json = '{"addon": "' + $slug + '"}'
$ip = $settings.HomeAssistant.Host
$port = $settings.HomeAssistant.Port

$token = (Get-Content src/appsettings.Development.json | ConvertFrom-Json).HomeAssistant.Token

# Point to the HA PowerShell Module
Unblock-File .\src\Home-Assistant\Home-Assistant.psd1
Unblock-File .\src\Home-Assistant\Home-Assistant.psm1
Import-Module .\src\Home-Assistant

New-HomeAssistantSession -ip  $ip -port $port -token $token

Remove-Item -Recurse -Force \\$ip\config\$version\*
dotnet publish -c Release src/HomeAutomation.csproj -o \\$ip\config\$version

Invoke-HomeAssistantService -service hassio.addon_restart -json $json