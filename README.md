# Portable FTP server

[![Build status](https://build.fubar-dev.de/app/rest/builds/buildType:%28id:FtpServer_ReleaseBuild%29/statusIcon.svg)](https://build.fubar-dev.com/project.html?projectId=FtpServer)

This FTP server is written as .NET Standard 2.0 library and has an
abstract file system which allows e.g. Google Drive as backend.

# License

The library is released under the [![MIT license](https://img.shields.io/github/license/mashape/apistatus.svg)](http://opensource.org/licenses/MIT).

# Prerequisites

## Compilation

* Visual Studio 2017 / C# 7.3

## Using

* Visual Studio 2017
* .NET Standard 2.0

## NuGet packages

| Description				| Badge |
|---------------------------|-------|
| `FubarDev.FtpServer`: Core library				| [![FubarDev.FtpServer](https://img.shields.io/nuget/vpre/FubarDev.FtpServer.svg)](https://www.nuget.org/packages/FubarDev.FtpServer) |
| `FD.FS.Abstractions`: Basic types				| [![FubarDev.FtpServer.Abstractions](https://img.shields.io/nuget/vpre/FubarDev.FtpServer.Abstractions.svg)](https://www.nuget.org/packages/FubarDev.FtpServer.Abstractions) |
| `FD.FS.FileSystem.DotNet`: `System.IO`-based file system	| [![FubarDev.FtpServer.FileSystem.DotNet](https://img.shields.io/nuget/vpre/FubarDev.FtpServer.FileSystem.DotNet.svg)](https://www.nuget.org/packages/FubarDev.FtpServer.FileSystem.DotNet) |
| `FD.FS.FileSystem.GoogleDrive`: Google Drive as file system	| [![FubarDev.FtpServer.FileSystem.GoogleDrive](https://img.shields.io/nuget/vpre/FubarDev.FtpServer.FileSystem.GoogleDrive.svg)](https://www.nuget.org/packages/FubarDev.FtpServer.FileSystem.GoogleDrive) |
| `FD.FS.FileSystem.InMemory`: In-memory file system	| [![FubarDev.FtpServer.FileSystem.InMemory](https://img.shields.io/nuget/vpre/FubarDev.FtpServer.FileSystem.InMemory.svg)](https://www.nuget.org/packages/FubarDev.FtpServer.FileSystem.InMemory) |

# Example FTP server

## Creating the project

```bash
dotnet new console
dotnet add package FubarDev.FtpServer.FileSystem.DotNet
dotnet add package FubarDev.FtpServer
dotnet add package Microsoft.Extensions.DependencyInjection
```

## Contents of `Main` in Program.cs

```csharp
// Setup dependency injection
var services = new ServiceCollection();

// use %TEMP%/TestFtpServer as root folder
services.Configure<DotNetFileSystemOptions>(opt => opt
    .RootPath = Path.Combine(Path.GetTempPath(), "TestFtpServer"));

// Add FTP server services
// DotNetFileSystemProvider = Use the .NET file system functionality
// AnonymousMembershipProvider = allow only anonymous logins
services.AddFtpServer(builder => builder
    .UseDotNetFileSystem() // Use the .NET file system functionality
    .EnableAnonymousAuthentication()); // allow anonymous logins

// Configure the FTP server
services.Configure<FtpServerOptions>(opt => opt.ServerAddress = "127.0.0.1");

// Build the service provider
using (var serviceProvider = services.BuildServiceProvider())
{
    // Initialize the FTP server
    var ftpServerHost = serviceProvider.GetRequiredService<IFtpServerHost>();

    // Start the FTP server
    ftpServerHost.StartAsync().Wait();
    
    Console.WriteLine("Press ENTER/RETURN to close the test application.");
    Console.ReadLine();
    
    // Stop the FTP server
    ftpServerHost.StopAsync().Wait();
}
```
