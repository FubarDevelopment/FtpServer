---
uid: upgrade-to-3.0
title: Upgrade from 2.x to 3.0
---

- [Overview](#overview)
  - [File system changes](#file-system-changes)
  - [Membership provider changes](#membership-provider-changes)
  - [FTP command extensions changes](#ftp-command-extensions-changes)
  - [Connection data changes](#connection-data-changes)
  - [FTP command collection changes](#ftp-command-collection-changes)
- [Changelog](#changelog)
  - [What's new?](#whats-new)
  - [What's changed?](#whats-changed)
  - [What's fixed?](#whats-fixed)
- [A look into the future](#a-look-into-the-future)

# Overview

After the upgrade 3.0, you'll see that the `IFtpServer.Start` and `IFtpServer.Stop` functions are
deprecated. Please query the [`IFtpServerHost`](xref:FubarDev.FtpServer.IFtpServerHost) instead and
use the [`StartAsync`](xref:FubarDev.FtpServer.IFtpServerHost.StartAsync(System.Threading.CancellationToken))
and [`StopAsync`](xref:FubarDev.FtpServer.IFtpServerHost.StopAsync(System.Threading.CancellationToken)) functions instead.

You will notice breaking changes in the following areas:

- File system
- Membership provider
- FTP command extensions
- Connection data
- FTP command collection

## File system changes

The only change for file systems is that
[`IFileSystemClassFactory.Create`](xref:FubarDev.FtpServer.FileSystem.IFileSystemClassFactory.Create(FubarDev.FtpServer.IAccountInformation))
now requires an [`IAccountInformation`](xref:FubarDev.FtpServer.IAccountInformation) parameter. This parameter provides more
information in case a file system implementation needs it.

## Membership provider changes

The membership provider is now asynchronous which means that the `ValidateUser` function was
renamed to [`ValidateUserAsync`](xref:FubarDev.FtpServer.AccountManagement.IMembershipProvider.ValidateUserAsync(System.String,System.String)).
Everything else is the same.

## FTP command extensions changes

The command extensions cannot be returned by `IFtpCommandHandler.GetExtensions()` anymore. The extensions were moved to
their own file and the default extensions are automatically registered as service.

## Connection data changes

The connection datas `IsAnonymous` property is obsolete. An anonymous user is now detected by testing if
the [`FtpConnectionData.User`](xref:FubarDev.FtpServer.FtpConnectionData.User)
implements [`IAnonymousFtpUser`](xref:FubarDev.FtpServer.AccountManagement.IAnonymousFtpUser).

## FTP command collection changes

We're now using `ReadOnlySpan` for both [`TelnetInputParser`](xref:FubarDev.FtpServer.TelnetInputParser`1)
and [`FtpCommandCollector`](xref:FubarDev.FtpServer.FtpCommandCollector).

# Changelog

## What's new?

- In-memory file system
- Passive data connection port range (contribution from 40three GmbH)
- New [`IFtpServerHost`](xref:FubarDev.FtpServer.IFtpServerHost) interface
- New [`IFtpService`](xref:FubarDev.FtpServer.IFtpService) interface which allows easy integration into ASP.NET Core
- New [`IAccountInformation`](xref:FubarDev.FtpServer.IAccountInformation) interface
- New [`IAnonymousFtpUser`](xref:FubarDev.FtpServer.AccountManagement.IAnonymousFtpUser) interface

## What's changed?

- Google drive upload without background uploader
- BREAKING: The FTP commands are now registered as singletons to improve performance
- BREAKING: Usage of `ReadOnlySpan` in the FTP command collector
- BREAKING: [`IFileSystemClassFactory.Create`](xref:FubarDev.FtpServer.FileSystem.IFileSystemClassFactory.Create(FubarDev.FtpServer.IAccountInformation))
  takes an [`IAccountInformation`](xref:FubarDev.FtpServer.IAccountInformation)
- BREAKING: The [`IMembershipProvider`](xref:FubarDev.FtpServer.AccountManagement.IMembershipProvider) is now asynchronous
- BREAKING: `FtpConnectionData.IsAnonymous` is obsolete, the anonymous user is now of type [`IAnonymousFtpUser`](xref:FubarDev.FtpServer.AccountManagement.IAnonymousFtpUser)
- The `IFtpCommandHandler.GetExtensions()` is now deprecated as all extensions that were previously returned here have
  their own implementation now

## What's fixed?

- AUTH TLS fails gracefully when no SSL certificate is configured
- `SITE BLST` works again

# A look into the future

The 4.x version will drop support for .NET Standard 1.3 and - eventually - .NET 4.6.1 as
the FTP Server will be reimplemented as `ConnectionHandler` which will result into the following
improvements:

- Easy hosting in an ASP.NET Core application
- Usage of pipelines when possible (`AUTH TLS` might cause problems)
- Using the ASP.NET Core connection state management
