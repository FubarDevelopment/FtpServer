# Portable FTP server

[![Build Status](https://dev.azure.com/fubar-development/ftp-server/_apis/build/status/FubarDevelopment.FtpServer?branchName=master)](https://dev.azure.com/fubar-development/ftp-server/_build/latest?definitionId=5&branchName=master)

This FTP server is written as .NET Standard 2.0 library and has an
abstract file system which allows e.g. Google Drive as backend.

# License

The library is released under the [![MIT license](https://img.shields.io/github/license/mashape/apistatus.svg)](http://opensource.org/licenses/MIT).

# Support the development

[![Patreon](https://img.shields.io/endpoint.svg?url=https:%2F%2Fshieldsio-patreon.herokuapp.com%2FFubarDevelopment&style=for-the-badge)](https://www.patreon.com/FubarDevelopment)

# Prerequisites

## Compilation

* Visual Studio 2022 / C# 8.0

## Using

* Visual Studio 2022
* .NET Standard 2.0 (everything **except** sample application, PAM authentication)
* .NET Core 3.1 (PAM authentication)

## NuGet packages

| Package name                                   | Description                   | Badge |
|------------------------------------------------|-------------------------------|-------|
| `FubarDev.FtpServer`                           | Core library                  | [![FubarDev.FtpServer](https://img.shields.io/nuget/vpre/FubarDev.FtpServer.svg)](https://www.nuget.org/packages/FubarDev.FtpServer) |
| `FubarDev.FtpServer.Abstractions`              | Basic types                   | [![FubarDev.FtpServer.Abstractions](https://img.shields.io/nuget/vpre/FubarDev.FtpServer.Abstractions.svg)](https://www.nuget.org/packages/FubarDev.FtpServer.Abstractions) |
| `FubarDev.FtpServer.FileSystem.DotNet`         | `System.IO`-based file system | [![FubarDev.FtpServer.FileSystem.DotNet](https://img.shields.io/nuget/vpre/FubarDev.FtpServer.FileSystem.DotNet.svg)](https://www.nuget.org/packages/FubarDev.FtpServer.FileSystem.DotNet) |
| `FubarDev.FtpServer.FileSystem.GoogleDrive`    | Google Drive as file system   | [![FubarDev.FtpServer.FileSystem.GoogleDrive](https://img.shields.io/nuget/vpre/FubarDev.FtpServer.FileSystem.GoogleDrive.svg)](https://www.nuget.org/packages/FubarDev.FtpServer.FileSystem.GoogleDrive) |
| `FubarDev.FtpServer.FileSystem.InMemory`       | In-memory file system         | [![FubarDev.FtpServer.FileSystem.InMemory](https://img.shields.io/nuget/vpre/FubarDev.FtpServer.FileSystem.InMemory.svg)](https://www.nuget.org/packages/FubarDev.FtpServer.FileSystem.InMemory) |
| `FubarDev.FtpServer.FileSystem.Unix`           | Unix file system              | [![FubarDev.FtpServer.FileSystem.Unix](https://img.shields.io/nuget/vpre/FubarDev.FtpServer.FileSystem.Unix.svg)](https://www.nuget.org/packages/FubarDev.FtpServer.FileSystem.Unix) |
| `FubarDev.FtpServer.MembershipProvider.Pam`    | PAM membership provider       | [![FubarDev.FtpServer.MembershipProvider.Pam](https://img.shields.io/nuget/vpre/FubarDev.FtpServer.MembershipProvider.Pam.svg)](https://www.nuget.org/packages/FubarDev.FtpServer.MembershipProvider.Pam) |

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
    ftpServerHost.StartAsync(CancellationToken.None).Wait();

    Console.WriteLine("Press ENTER/RETURN to close the test application.");
    Console.ReadLine();

    // Stop the FTP server
    ftpServerHost.StopAsync(CancellationToken.None).Wait();
}
```
