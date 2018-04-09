# Portable FTP server

[![Build status](https://build.fubar-dev.de/app/rest/builds/buildType:%28id:FtpServer_ReleaseBuild%29/statusIcon)](https://build.fubar-dev.com/project.html?projectId=FtpServer)

This FTP server is written as .NET Standard 2.0 library and has an
abstract file system which allows e.g. Google Drive as backend.

# License

The library is released under the [![MIT license](https://img.shields.io/github/license/mashape/apistatus.svg)](http://opensource.org/licenses/MIT).

# Prerequisites

## Compilation

* Visual Studio 2017 / C# 7.2

## Using

* Visual Studio 2017
* .NET Standard 2.0

## NuGet packages

| Description				| Badge |
|---------------------------|-------|
| Core library				| [![FubarDev.FtpServer](https://img.shields.io/nuget/v/FubarDev.FtpServer.svg)](https://www.nuget.org/packages/FubarDev.FtpServer) |
| Virtual FTP file system	| [![FubarDev.FtpServer.FileSystem](https://img.shields.io/nuget/v/FubarDev.FtpServer.FileSystem.svg)](https://www.nuget.org/packages/FubarDev.FtpServer.FileSystem) |
| Google Drive support		| [![FubarDev.FtpServer.FileSystem.GoogleDrive](https://img.shields.io/nuget/v/FubarDev.FtpServer.FileSystem.GoogleDrive.svg)](https://www.nuget.org/packages/FubarDev.FtpServer.FileSystem.GoogleDrive) |
| OneDrive support			| [![FubarDev.FtpServer.FileSystem.OneDrive](https://img.shields.io/nuget/v/FubarDev.FtpServer.FileSystem.OneDrive.svg)](https://www.nuget.org/packages/FubarDev.FtpServer.FileSystem.OneDrive) |
| Account management		| [![FubarDev.FtpServer.AccountManagement](https://img.shields.io/nuget/v/FubarDev.FtpServer.AccountManagement.svg)](https://www.nuget.org/packages/FubarDev.FtpServer.AccountManagement) |
| FTPS (TLS) Support       	| [![FubarDev.FtpServer.AuthTls](https://img.shields.io/nuget/v/FubarDev.FtpServer.AuthTls.svg)](https://www.nuget.org/packages/FubarDev.FtpServer.AuthTls) |

# Example FTP server

```csharp
// Setup dependency injection
var services = new ServiceCollection();

// use %TEMP%/TestFtpServer as root folder
services.Configure<DotNetFileSystemOptions>(opt => opt.RootPath = Path.Combine(Path.GetTempPath(), "TestFtpServer"));

// Add FTP server services
// DotNetFileSystemProvider = Use the .NET file system functionality
// AnonymousMembershipProvider = allow only anonymous logins
services.AddFtpServer<DotNetFileSystemProvider, AnonymousMembershipProvider>();

// Configure the FTP server
services.Configure<FtpServerOptions>(opt => opt.ServerAddress = "127.0.0.1");

// Build the service provider
using (var serviceProvider = services.BuildServiceProvider()) {

    // Initialize the FTP server
    var ftpServer = serviceProvider.GetRequiredService<FtpServer>();

    // Start the FTP server
    ftpServer.Start();
    
    Console.WriteLine("Press ENTER/RETURN to close the test application.");
    Console.ReadLine();
    
    // Stop the FTP server
    ftpServer.Stop();
}
```
