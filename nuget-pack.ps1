[OutputType([void])]
param(
	[Parameter()]
	$version = "1.0.0-beta21",
	[Parameter()]
	$config = "Release"
)

Remove-Item *.nupkg

$nuspecFiles = get-childitem FubarDev.FtpServer*\*.nuspec
ForEach ($nuspecFile in $nuspecFiles)
{
	$csFile = [System.IO.Path]::ChangeExtension($nuspecFile, ".csproj")
	& nuget pack "$csFile" -Properties "Configuration=$config;Version=$version" -IncludeReferencedProjects
}
