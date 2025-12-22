#Requires -RunAsAdministrator
param (
    [string]$Stage
)

if ([string]::IsNullOrWhiteSpace($Stage)) {
    Write-Error "<Stage> argument is required (one of dev/green/prod)."
    exit 1
}

Add-Type -AssemblyName 'System.Web'
. ./GenerateCertificate.ps1

$secrets = ""
$secretsRancher = ""

$postgresPassword = -Join("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".tochararray() | Get-Random -Count 10 | % {[char]$_})
$replicationPostgresPassword = -Join("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".tochararray() | Get-Random -Count 10 | % {[char]$_})

$SigningCertificate_Password = -Join("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".tochararray() | Get-Random -Count 14 | % {[char]$_})
$SigningCertificate_CertificateBytes = GenerateCertificateAsByteArray($SigningCertificate_Password)
$SigningCertificate_Base64Certificate = [Convert]::ToBase64String($SigningCertificate_CertificateBytes)

$EncryptionCertificate_Password = -Join("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".tochararray() | Get-Random -Count 14 | % {[char]$_})
$EncryptionCertificate_CertificateBytes = GenerateCertificateAsByteArray($EncryptionCertificate_Password)
$EncryptionCertificate_Base64Certificate = [Convert]::ToBase64String($EncryptionCertificate_CertificateBytes)

$template = Get-Content -path './.env_template' -Raw
$template = $template -replace "-STAGE", "-$Stage"

$Hangfire__DashboardPassword = -Join("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".tochararray() | Get-Random -Count 14 | % {[char]$_})
$DefaultUser__Password = -Join("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".tochararray() | Get-Random -Count 14 | % {[char]$_})
$SignUrl__Secret = -Join("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".tochararray() | Get-Random -Count 32 | % {[char]$_})

$secrets = $secrets + @"
OpenId__SigningCertificate__Password=$SigningCertificate_Password
OpenId__EncryptionCertificate__Password=$EncryptionCertificate_Password
superUserPassword=$postgresPassword
replicationUserPassword=$replicationPostgresPassword
Hangfire__DashboardPassword=$Hangfire__DashboardPassword
DefaultUser__Password=$DefaultUser__Password
SignUrl__Secret=$SignUrl__Secret
Serilog__Elastic__Password=''
"@

$secretsRancher = $secretsRancher + @"
OpenId__SigningCertificate__Password: $([Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($SigningCertificate_Password)))
OpenId__EncryptionCertificate__Password: $([Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($EncryptionCertificate_Password)))
superUserPassword: $([Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($postgresPassword)))
replicationUserPassword: $([Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($replicationPostgresPassword)))
Hangfire__DashboardPassword: $([Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($Hangfire__DashboardPassword)))
DefaultUser__Password: $([Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($DefaultUser__Password)))
SignUrl__Secret: $([Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($SignUrl__Secret)))
Serilog__Elastic__Password: 
"@



$newContent	= $template + @"
export OpenId__SigningCertificate__Base64Certificate=$SigningCertificate_Base64Certificate
export OpenId__EncryptionCertificate__Base64Certificate=$EncryptionCertificate_Base64Certificate
"@

$secrets | Set-Content -Path "./$Stage.secrets.env"
Write-Output "secrets.$Stage.env file is generated"
Write-Output $secrets

$secretsRancher | Set-Content -Path "./$Stage.secrets.rancher.env"
Write-Output "secrets.rancher.$Stage.env file is generated"
Write-Output $secretsRancher

$newContent | Set-Content -Path "./$Stage.env"
Write-Output "$Stage.env file is generated"
Write-Output $newContent

Copy-Item -Path "./$Stage.env" -Destination "../k8s/aks-rancher/stages/$Stage.env"
Copy-Item -Path "./$Stage.secrets.env" -Destination "../k8s/aks-rancher/stages/$Stage.secrets.env"
Copy-Item -Path "./$Stage.secrets.rancher.env" -Destination "../k8s/aks-rancher/stages/$Stage.secrets.rancher.env"
