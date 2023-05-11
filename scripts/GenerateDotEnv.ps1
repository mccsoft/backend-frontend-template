#Requires -RunAsAdministrator

Add-Type -AssemblyName 'System.Web'
. ./GenerateCertificate.ps1

$postgresPassword = [System.Web.Security.Membership]::GeneratePassword(14, 0)

$SigningCertificate_Password = [System.Web.Security.Membership]::GeneratePassword(14, 0)
$SigningCertificate_CertificateBytes = GenerateCertificateAsByteArray($SigningCertificate_Password)
$SigningCertificate_Base64Certificate = [Convert]::ToBase64String($SigningCertificate_CertificateBytes)

$EncryptionCertificate_Password = [System.Web.Security.Membership]::GeneratePassword(14, 0)
$EncryptionCertificate_CertificateBytes = GenerateCertificateAsByteArray($EncryptionCertificate_Password)
$EncryptionCertificate_Base64Certificate = [Convert]::ToBase64String($EncryptionCertificate_CertificateBytes)

$template = Get-Content -path './.env_template' -Raw

$newContent	= $template + `
'POSTGRES_PASSWORD='+$postgresPassword+'
ConnectionStrings__DefaultConnection=Server=postgres;Database=template_app;Port=5432;Username=postgres;Password='+$postgresPassword+';Pooling=true;Keepalive=5;Command Timeout=60;Trust Server Certificate=true
OpenId__SigningCertificate__Base64Certificate='+$SigningCertificate_Base64Certificate+'
OpenId__SigningCertificate__Password='+$SigningCertificate_Password+'
OpenId__EncryptionCertificate__Base64Certificate='+$EncryptionCertificate_Base64Certificate+'
OpenId__EncryptionCertificate__Password='+$EncryptionCertificate_Password+'
Hangfire__DashboardPassword='+[System.Web.Security.Membership]::GeneratePassword(14, 0)+'
DefaultUser__Password='+[System.Web.Security.Membership]::GeneratePassword(14, 0)+'
SignUrl__Secret='+[System.Web.Security.Membership]::GeneratePassword(32, 4)+'
'

$newContent | Set-Content -Path './.env'
Write-Output ".env file is generated"
Write-Output $newContent
