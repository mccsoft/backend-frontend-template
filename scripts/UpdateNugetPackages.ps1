$configFiles = Get-ChildItem ../ *.csproj -rec
foreach ($file in $configFiles)
{
	
    $initialContent = Get-Content $file.PSPath
    $newContent	= $initialContent | 
		# Non-Microsoft libraries
		
		# Hangfire
		Foreach-Object { $_ -replace '(<PackageReference Include="Hangfire.*?" Version=")(.*?)"', '${1}1.7.24"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Hangfire.Dashboard.BasicAuthorization" Version=")(.*?)"', '${1}1.0.2"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Hangfire.PostgreSql" Version=")(.*?)"', '${1}1.8.5.4"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="NeinLinq.EntityFrameworkCore" Version=")(.*?)"', '${1}5.1.0"' } |
		
		# Npgsql is usually a bit lacking behind .net core versions
		Foreach-Object { $_ -replace '(<PackageReference Include="Npgsql.Json.NET" Version=")(.*?)"', '${1}5.0.7"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version=")(.*?)"', '${1}5.0.7"' } |	
		
		# Microsoft libraries, and libs released together with .net core sdk (with matching versions)
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.AspNetCore.Mvc.NewtonsoftJson" Version=")(.*?)"', '${1}5.0.10"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version=")(.*?)"', '${1}5.0.10"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version=")(.*?)"', '${1}5.0.10"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.EntityFrameworkCore.Relational" Version=")(.*?)"', '${1}5.0.10"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version=")(.*?)"', '${1}5.0.10"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version=")(.*?)"', '${1}5.0.10"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.EntityFrameworkCore" Version=")(.*?)"', '${1}5.0.10"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore" Version=")(.*?)"', '${1}5.0.10"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version=")(.*?)"', '${1}5.0.10"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.AspNetCore.Authorization" Version=")(.*?)"', '${1}5.0.10"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version=")(.*?)"', '${1}5.0.10"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.Extensions.Http.Polly" Version=")(.*?)"', '${1}5.0.10"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.Extensions.Caching.Memory" Version=")(.*?)"', '${1}5.0.10"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.AspNetCore.TestHost" Version=")(.*?)"', '${1}5.0.10"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version=")(.*?)"', '${1}5.0.10"' } | 
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.AspNetCore.ApiAuthorization.IdentityServer" Version=")(.*?)"', '${1}5.0.10"' } | 
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.AspNetCore.Authentication.*?" Version=")(.*?)"', '${1}5.0.10"' } | 
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore" Version=")(.*?)"', '${1}5.0.10"' } | 
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version=")(.*?)"', '${1}5.0.10"' } | 
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.AspNetCore.Identity.UI" Version=")(.*?)"', '${1}5.0.10"' } | 
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.AspNetCore.SpaServices.Extensions" Version=")(.*?)"', '${1}5.0.10"' } | 
		# Below library versions are usually released much rare
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.Extensions.Logging.Debug" Version=")(.*?)"', '${1}5.0.0"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.Extensions.Configuration.Json" Version=")(.*?)"', '${1}5.0.0"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.Extensions.Options.ConfigurationExtensions" Version=")(.*?)"', '${1}5.0.0"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.Extensions.Options" Version=")(.*?)"', '${1}5.0.0"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version=")(.*?)"', '${1}5.0.0"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.Extensions.Logging" Version=")(.*?)"', '${1}5.0.0"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.Extensions.Http" Version=")(.*?)"', '${1}5.0.0"' } |
		Foreach-Object { $_ -replace '(<PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version=")(.*?)"', '${1}5.0.0"' }
		
	if ((-join $newContent) -ne (-join $initialContent)) {
		$newContent | Set-Content $file.PSPath
	}
}