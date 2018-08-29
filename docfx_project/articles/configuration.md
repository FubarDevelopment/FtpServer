---
uid: configuration
title: FTP Server Configuration
---

# Introduction

The configuration is split into several parts:

* Services
* [IFtpServerBuilder](xref:FubarDev.FtpServer.IFtpServerBuilder) is the FTP Server configuration builder
* [FtpServerOptions](xref:FubarDev.FtpServer.FtpServerOptions) for the FTP server transport configuration
* [FtpConnectionOptions](xref:FubarDev.FtpServer.FtpConnectionOptions) to configure the default control connection text encoding
* [AuthTlsOptions](xref:FubarDev.FtpServer.AuthTlsOptions) for the FTPS configuration
* [DotNetFileSystemOptions](xref:FubarDev.FtpServer.FileSystem.DotNet.DotNetFileSystemOptions) to configure the file system access
* [SystCommandOptions](xref:FubarDev.FtpServer.SystCommandOptions) to specify the behavior of the SYST command

# Services

You use a `ServiceCollection` and add your services to, e.g.:

```csharp
var services = new ServiceCollection()
    .AddFtpServer(builder => builder
        .EnableAnonymousAuthentication()
        .UseDotNetFileSystem());
```

This is also the absolute minimum configuration for the FTP server.

# [IFtpServerBuilder](xref:FubarDev.FtpServer.IFtpServerBuilder)

This is the place where other services are configured that will be used by the FTP server. This are the options currently available:

- `builder.EnableAnonymousAuthentication()`: Enables anonymous authentication
- `builder.UseDotNetFileSystem()`: Uses a System.IO-based file systme
- `builder.UseGoogleDrive(...)`: Uses Google Drive as virtual file system

# [FtpServerOptions](xref:FubarDev.FtpServer.FtpServerOptions)

This configures the address/host/port used by the server to listen for connections.

The default value is `localhost` as address (IPv4 and IPv6 - if available) and 21 as port.

# [FtpConnectionOptions](xref:FubarDev.FtpServer.FtpConnectionOptions)

This may be used to configure a different default character encoding. The default value is ASCII.

# [AuthTlsOptions](xref:FubarDev.FtpServer.AuthTlsOptions)

This configuration option allows the specification of an `X509Certificate2` (with private key) that will be used to initiate encrypted connections.

# [DotNetFileSystemOptions](xref:FubarDev.FtpServer.FileSystem.DotNet.DotNetFileSystemOptions)

Used to configure the behavior of the System.IO-based file system access, like:

- The path to the root directory
- Usage of the user ID as subdirectory
- Is deletion of non-empty directories allowed?

# [SystCommandOptions](xref:FubarDev.FtpServer.SystCommandOptions)

Here you can specify the operating system to be returned by the SYST command.
