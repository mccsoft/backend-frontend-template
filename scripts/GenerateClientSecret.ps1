function GenerateClientSecret()
{
    $unid = New-Guid
    $c = new-object System.Security.Cryptography.SHA256Managed
    $hash = $c.ComputeHash([Text.Encoding]::ASCII.GetBytes($unid))
    return ($unid, $([Convert]::ToBase64String($hash)));
}
$isDotSourced = $MyInvocation.InvocationName -eq '.' -or $MyInvocation.Line -eq ''
if (!$isDotSourced)
{
    ($client, $secret) = GenerateClientSecret
    Write-Host $client, $secret
}