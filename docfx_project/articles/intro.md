# Install from NuGet.org

The project is split into multiple packages:

Package | Description
--------|-------------
FubarDev.FtpServer | The FTP server implementation
FubarDev.FtpServer.Abstractions | The base classes/interfaces; Reference this to implement custom commands, file systems, authentication
FubarDev.FtpServer.Commands | The FTP commands implemented for this FTP server
FubarDev.FtpServer.FileSystem.DotNet | The System.IO-based file system access
FubarDev.FtpServer.FileSystem.GoogleDrive | Google Drive as virtual file system

## Getting Started

* Check out the tour of FluentMigrator in our [Quickstart](xref:quickstart)

## Basic topics

* [Configuration](xref:configuration)
* [Logging](xref:logging)
* [Authentication](xref:authentication)
* [File Systems](xref:file-systems)

## Advanced topics

* [Logging](xref:logging)
* [FTPS](xref:ftps)
* [Google Drive](xref:google-drive)
* [Custom File System Development](xref:custom-file-system)
* [Custom Account Management](xref:custom-account-management)
