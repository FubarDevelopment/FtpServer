# API documentation

This is the documentation for the current version of the FTP server.

The most important parts of the API are:

* [IFtpServer](xref:FubarDev.FtpServer.IFtpServer) the FTP server itself
* [FtpServerOptions](xref:FubarDev.FtpServer.FtpServerOptions) for the FTP server transport configuration
* [FtpConnectionOptions](xref:FubarDev.FtpServer.FtpConnectionOptions) to configure the default control connection text encoding
* [AuthTlsOptions](xref:FubarDev.FtpServer.AuthTlsOptions) for the FTPS configuration
* [DotNetFileSystemOptions](xref:FubarDev.FtpServer.FileSystem.DotNet.DotNetFileSystemOptions) to configure the file system access
