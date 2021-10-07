$configFiles = Get-ChildItem ./ *.csproj -rec
foreach ($file in $configFiles)
{
    $initialContent = Get-Content $file.PSPath
    $newContent	= $initialContent | 
		Foreach-Object { $_ -replace '(<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version=")(.*?)"', '${1}5.0.6"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.*?" Version=")(.*?)"', '${1}5.0.6"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.NET.Test.Sdk" Version=")(.*?)"', '${1}16.9.4"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.Extensions.Http" Version=")(.*?)"', '${1}5.0.0"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.Extensions.Logging" Version=")(.*?)"', '${1}5.0.0"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version=")(.*?)"', '${1}5.0.0"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version=")(.*?)"', '${1}5.0.0"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.Extensions.FileProviders.Abstractions" Version=")(.*?)"', '${1}5.0.0"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.Extensions.Logging.Debug" Version=")(.*?)"', '${1}5.0.0"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.Extensions.Configuration.Json" Version=")(.*?)"', '${1}5.0.0"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.AspNetCore.Mvc.DataAnnotations" Version=")(.*?)"', '${1}2.2.0"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.AspNetCore.Http.Abstractions" Version=")(.*?)"', '${1}2.2.0"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.AspNetCore.Hosting.Abstractions" Version=")(.*?)"', '${1}2.2.0"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.AspNetCore.Mvc.Abstractions" Version=")(.*?)"', '${1}2.2.0"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.AspNetCore.Routing.Abstractions" Version=")(.*?)"', '${1}2.2.0"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.AspNetCore.Mvc.Core" Version=")(.*?)"', '${1}2.2.0"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.AspNetCore.SpaServices" Version=")(.*?)"', '${1}3.1.15"' }
		
	if ((-join $newContent) -ne (-join $initialContent)) {
		$newContent | Set-Content $file.PSPath
	}
}