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
OpenId__SigningCertificate__Base64Certificate='+$SigningCertificate_Base64Certificate+'
OpenId__SigningCertificate__Password='+$SigningCertificate_Password+'
OpenId__EncryptionCertificate__Base64Certificate='+$EncryptionCertificate_Base64Certificate+'
OpenId__EncryptionCertificate__Password='+$EncryptionCertificate_Password+'
Hangfire__DashboardPassword='+[System.Web.Security.Membership]::GeneratePassword(14, 0)+'
DefaultUser__Password='+[System.Web.Security.Membership]::GeneratePassword(14, 0)+'
SignUrl__Secret='+[System.Web.Security.Membership]::GeneratePassword(20, 4)+'
'

$newContent | Set-Content -Path './.env.base'
