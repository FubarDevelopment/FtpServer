[OutputType([void])]
param(
	[Parameter()]
	$config = "Release"
)

Remove-Item *.nupkg

$nuspecFiles = get-childitem FubarDev.FtpServer*\*.nuspec
ForEach ($nuspecFile in $nuspecFiles)
{
	$csFile = [System.IO.Path]::ChangeExtension($nuspecFile, ".csproj")
	& nuget pack "$csFile" -Properties "Configuration=$config" -IncludeReferencedProjects
	write-host $csFile
}
