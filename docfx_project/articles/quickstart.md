---
uid: quickstart
title: Your first FTP server
---

# Creating a project

```bash
mkdir ftpserver
cd ftpserver
dotnet new console
```

# Adding the NuGet packages

```bash
# For dependency injection support (required)
dotnet add package Microsoft.Extensions.DependencyInjection -v 2.1.0

# For the main FTP server
dotnet add package FubarDev.FtpServer -v 2.0.0-beta.36

# For the System.IO-based file system access
dotnet add package FubarDev.FtpServer.FileSystem.DotNet -v 2.0.0-beta.36
```

# Using the FTP server

Change your `Program.cs` to the following code:

[!code-cs[Program.cs](../code-snippets/quickstart/Program.cs "The FTP server")]

# Starting the FTP server

```bash
dotnet run
```

Now your FTP server should be accessible at `127.0.0.1:21`.
