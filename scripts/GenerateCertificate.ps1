#Requires -RunAsAdministrator

function GenerateCertificateAsByteArray($password) {
    
    $rootCA = New-SelfSignedCertificate -CertStoreLocation Cert:\LocalMachine\My -DnsName "IdentityServerCertificate" -FriendlyName "IdentityServerCertificate" -NotAfter (Get-Date).AddYears(100) -HashAlgorithm SHA256
    Move-Item (Join-Path Cert:\LocalMachine\My $rootCA.Thumbprint) -Destination Cert:\LocalMachine\Root
    
    $CertPassword = ConvertTo-SecureString -String $password -Force -AsPlainText

    $fileName = [IO.Path]::Combine((Get-Location), 'temp.pfx')
    
    Export-PfxCertificate -Cert (Join-Path Cert:\LocalMachine\Root $rootCA.Thumbprint) -FilePath $fileName -Password $CertPassword  | Out-Null

    $bytes = [IO.File]::ReadAllBytes($fileName)
    
    Remove-Item $fileName  | Out-Null
    
    return $bytes
}

$isDotSourced = $MyInvocation.InvocationName -eq '.' -or $MyInvocation.Line -eq ''
if (!$isDotSourced)
{
    Add-Type -AssemblyName 'System.Web'
    $password = [System.Web.Security.Membership]::GeneratePassword(14, 0)

    $cert = GenerateCertificateAsByteArray($password)
    [IO.File]::WriteAllBytes([IO.Path]::Combine((Get-Location), 'idsrv4cert.pfx'), $cert)
    Write-Host 'Password: ', $password
}