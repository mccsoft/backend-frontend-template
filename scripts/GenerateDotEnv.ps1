#Requires -RunAsAdministrator

Add-Type -AssemblyName 'System.Web'
. ./GenerateClientSecret.ps1
. ./GenerateCertificate.ps1

$postgresPassword = [System.Web.Security.Membership]::GeneratePassword(14, 0)

($WEB_CLIENT_KEY, $WEB_CLIENT_SECRET) = GenerateClientSecret
($MOBILE_CLIENT_KEY, $MOBILE_CLIENT_SECRET) = GenerateClientSecret

$IdentityServer_Password = [System.Web.Security.Membership]::GeneratePassword(14, 0)
$IdentityServer_CertificateBytes = GenerateCertificateAsByteArray($IdentityServer_Password)
$IdentityServer_Base64Certificate = [Convert]::ToBase64String($IdentityServer_CertificateBytes)

$template = Get-Content -path './.env_template' -Raw

$newContent	= $template + `
'WEB_CLIENT_KEY='+$WEB_CLIENT_KEY+'
WEB_CLIENT_SECRET='+$WEB_CLIENT_SECRET+'
MOBILE_CLIENT_SECRET='+$MOBILE_CLIENT_SECRET+'
POSTGRES_PASSWORD='+$postgresPassword+'
IdentityServer_Base64Certificate='+$IdentityServer_Base64Certificate+'
IdentityServer_Password='+$IdentityServer_Password+'
SIGN_URL_SECRET='+[System.Web.Security.Membership]::GeneratePassword(20, 4)+'
'


$newContent | Set-Content -Path './.env.base'

Write-Host 'Mobile Client Secret: ', $MOBILE_CLIENT_SECRET