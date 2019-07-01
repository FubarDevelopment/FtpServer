---
uid: quickstart-generic-host
title: Your first FTP server
---

# Creating a project

```bash
mkdir TestGenericHost
cd TestGenericHost
dotnet new console
```

# Adding the NuGet packages

```bash
# For dependency injection support (required)
dotnet add package Microsoft.Extensions.DependencyInjection

# For the main FTP server
dotnet add package FubarDev.FtpServer

# For the System.IO-based file system access
dotnet add package FubarDev.FtpServer.FileSystem.DotNet
```

# Create an `IHostedService` implementation

> [!IMPORTANT]
> This is only required for version 3.0, because the FTP server will
> provide a much tighter ASP.NET Core integration in a future release.

Create a new file named `HostedFtpService.cs`, which contains the following code:

[!code-cs[Program.cs](../code-snippets/quickstart-generic-host/HostedFtpService.cs "The wrapper for the hosted FTP server")]

# Using the FTP server

Change your `Program.cs` to the following code:

[!code-cs[Program.cs](../code-snippets/quickstart-generic-host/Program.cs "The FTP server")]

# Starting the FTP server

```bash
dotnet run
```

Now your FTP server should be accessible at `127.0.0.1:21`.
