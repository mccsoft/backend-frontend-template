#Requires -RunAsAdministrator

Add-Type -AssemblyName 'System.Web'
. ./GenerateCertificate.ps1

$postgresPassword = [System.Web.Security.Membership]::GeneratePassword(14, 0)

$IdentityServer_Password = [System.Web.Security.Membership]::GeneratePassword(14, 0)
$IdentityServer_CertificateBytes = GenerateCertificateAsByteArray($IdentityServer_Password)
$IdentityServer_Base64Certificate = [Convert]::ToBase64String($IdentityServer_CertificateBytes)

$template = Get-Content -path './.env_template' -Raw

$newContent	= $template + `
'POSTGRES_PASSWORD='+$postgresPassword+'
IdentityServer_Base64Certificate='+$IdentityServer_Base64Certificate+'
IdentityServer_Password='+$IdentityServer_Password+'
SIGN_URL_SECRET='+[System.Web.Security.Membership]::GeneratePassword(20, 4)+'
'

$newContent | Set-Content -Path './.env.base'
