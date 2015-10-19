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

$nuspecFiles = get-childitem FubarDev.FtpServer*\*.nuspec
ForEach ($nuspecFile in $nuspecFiles)
{
	$csFile = [System.IO.Path]::ChangeExtension($nuspecFile, ".csproj")
	& nuget pack "$csFile" -Properties $properties -IncludeReferencedProjects
}
