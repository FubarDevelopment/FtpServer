[OutputType([void])]
param(
	[Parameter()]
	$config = "Release",
	[Parameter()]
	$version = $null
)

Remove-Item *.nupkg

$properties = "Configuration=$config"
if ($version) {
	$properties = "$properties;Version=$version"
}

$nuspecFiles = get-childitem FubarDev.FtpServer*\*.nuspec | ? { $_.Name -notlike "*GoogleDrive*" -and $_.Name -notlike "*OneDrive*" }
ForEach ($nuspecFile in $nuspecFiles)
{
	$csFile = [System.IO.Path]::ChangeExtension($nuspecFile, ".csproj")
	& nuget pack "$csFile" -Properties $properties -IncludeReferencedProjects
}

$properties = "Configuration=$config"
if ($version) {
	$properties = "$properties;Version=${version}-beta"
}

$nuspecFiles = get-childitem FubarDev.FtpServer*\*.nuspec | ? { $_.Name -like "*GoogleDrive*" -or $_.Name -like "*OneDrive*" }
ForEach ($nuspecFile in $nuspecFiles)
{
	$csFile = [System.IO.Path]::ChangeExtension($nuspecFile, ".csproj")
	& nuget pack "$csFile" -Properties $properties -IncludeReferencedProjects
}
