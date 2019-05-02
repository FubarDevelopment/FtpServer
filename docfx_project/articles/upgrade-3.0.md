---
uid: upgrade-to-3.0
title: Upgrade from 2.x to 3.0
---

- [Overview](#overview)
  - [File system changes](#file-system-changes)
  - [Account management changes](#account-management-changes)
    - [Account directories queryable](#account-directories-queryable)
    - [Membership provider changes](#membership-provider-changes)
  - [FTP command extensions changes](#ftp-command-extensions-changes)
  - [Connection data changes](#connection-data-changes)
  - [FTP command collection changes](#ftp-command-collection-changes)
  - [Authorization/authentication as per RFC 2228](#authorizationauthentication-as-per-rfc-2228)
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

There are two important changes:

- [`IFileSystemClassFactory.Create`](xref:FubarDev.FtpServer.FileSystem.IFileSystemClassFactory.Create(FubarDev.FtpServer.IAccountInformation))
now requires an [`IAccountInformation`](xref:FubarDev.FtpServer.IAccountInformation) parameter
- The [`IUnixFileSystemEntry`](xref:FubarDev.FtpServer.FileSystem.IUnixFileSystemEntry) doesn't contain the `FileSystem` property anymore.

## Account management changes

### Account directories queryable

A new interface has been introduced to get the root and home directories for
a given user.

Type name | Description
----------|----------------------
[`SingleRootWithoutHomeAccountDirectoryQuery`](xref:FubarDev.FtpServer.AccountManagement.Directories.SingleRootWithoutHome.SingleRootWithoutHomeAccountDirectoryQuery) | Provides a single root for all users.
[`RootPerUserAccountDirectoryQuery`](xref:FubarDev.FtpServer.AccountManagement.Directories.RootPerUser.RootPerUserAccountDirectoryQuery) | Gives every user its own root directory. Useful, when - for example - the file system root was set to `/home`.
[`PamAccountDirectoryQuery`](xref:FubarDev.FtpServer.MembershipProvider.Pam.Directories.PamAccountDirectoryQuery) | Uses home directory information from PAM. The home directory can be configured to be the root instead.

### Membership provider changes

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

## Authorization/authentication as per RFC 2228

The authorization/authentication stack is new and implemented as
specified in the [RFC 2228](https://tools.ietf.org/rfc/rfc2228.txt).

This results in additional interfaces/extension points, like

- `IAuthorizationMechanism`
- `IAuthenticationMechanism`

There is also a new extension point for actions to be called when
the user is fully authorized: `IAuthorizationAction`. You can develop
your own action, but you should only use an [`IAuthorizationAction.Level`](xref:FubarDev.FtpServer.Authorization.IAuthorizationAction.Level)
below 1000. The values from 1000 (incl.) to 2000 (incl.) are reserved by
the FTP server and are used to initialize the FTP connection data.

# Changelog

## What's new?

- In-memory file system
- Unix file system
- Passive data connection port range (contribution from 40three GmbH)
- New [`IFtpServerHost`](xref:FubarDev.FtpServer.IFtpServerHost) interface
- New [`IFtpService`](xref:FubarDev.FtpServer.IFtpService) interface which allows easy integration into ASP.NET Core
- New [`IAccountInformation`](xref:FubarDev.FtpServer.IAccountInformation) interface
- New [`IAnonymousFtpUser`](xref:FubarDev.FtpServer.AccountManagement.IAnonymousFtpUser) interface
- New RFC 2228 compliant authentication/authorization
- Root and home directories for an account can be queried

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

The 4.x version will drop support for .NET Standard 1.3 and - most likely - .NET 4.6.1 as
the FTP Server will be reimplemented as `ConnectionHandler` which will result into the following
improvements:

- Easy hosting in an ASP.NET Core application
- Usage of pipelines when possible (`AUTH TLS` might cause problems)
- Using the ASP.NET Core connection state management
