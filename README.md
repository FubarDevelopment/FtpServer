# Portable FTP server

[![Build status](https://build.fubar-dev.de/app/rest/builds/buildType:%28id:FtpServer_ReleaseBuild%29/statusIcon)](https://build.fubar-dev.com/project.html?projectId=FtpServer)

This FTP server is written as PCL and has an abstract file system
which allows e.g. Google Drive as backend.

# License

The library is released under the [MIT license](http://opensource.org/licenses/MIT).

# Prerequisites

## Compilation

* Visual Studio 2015 / C# 6

## Using

* Visual Studio 2013 (maybe 2012 too)
* .NET 4.5, .NET Core 5, Windows 8, Windows Phone 8.1, Windows Phone Silverlight 8.0, Xamarin iOS/Android

## NuGet packages

| Description				| Badge |
|---------------------------|-------|
| Core library				| [![FubarDev.FtpServer](https://img.shields.io/nuget/v/FubarDev.FtpServer.svg)](https://www.nuget.org/packages/FubarDev.FtpServer) |
| Virtual FTP file system	| [![FubarDev.FtpServer.FileSystem](https://img.shields.io/nuget/v/FubarDev.FtpServer.FileSystem.svg)](https://www.nuget.org/packages/FubarDev.FtpServer.FileSystem) |
| Google Drive support		| [![FubarDev.FtpServer.FileSystem.GoogleDrive](https://img.shields.io/nuget/v/FubarDev.FtpServer.FileSystem.GoogleDrive.svg)](https://www.nuget.org/packages/FubarDev.FtpServer.FileSystem.GoogleDrive) |
| OneDrive support			| [![FubarDev.FtpServer.FileSystem.OneDrive](https://img.shields.io/nuget/v/FubarDev.FtpServer.FileSystem.OneDrive.svg)](https://www.nuget.org/packages/FubarDev.FtpServer.FileSystem.OneeDrive) |
| Account management		| [![FubarDev.FtpServer.AccountManagement](https://img.shields.io/nuget/v/FubarDev.FtpServer.AccountManagement.svg)](https://www.nuget.org/packages/FubarDev.FtpServer.AccountManagement) |
| FTPS (TLS) Support       	| [![FubarDev.FtpServer.AuthTls](https://img.shields.io/nuget/v/FubarDev.FtpServer.AuthTls.svg)](https://www.nuget.org/packages/FubarDev.FtpServer.AuthTls) |

# Example FTP server

```csharp
// allow only anonymous logins
var membershipProvider = new AnonymousMembershipProvider();

// use %TEMP%/TestFtpServer as root folder
var fsProvider = new DotNetFileSystemProvider(Path.Combine(Path.GetTempPath(), "TestFtpServer"), false);

// Initialize the FTP server
var ftpServer = new FtpServer(fsProvider, membershipProvider, "127.0.0.1");

// Start the FTP server
ftpServer.Start();

Console.WriteLine("Press ENTER/RETURN to close the test application.");
Console.ReadLine();

// Stop the FTP server
ftpServer.Stop();
```
