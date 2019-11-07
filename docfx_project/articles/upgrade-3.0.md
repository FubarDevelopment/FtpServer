---
uid: upgrade-to-3.0
title: Upgrade from 2.x to 3.0
---

- [Overview](#overview)
- [File system changes](#file-system-changes)
- [Authorization/authentication as per RFC 2228](#authorizationauthentication-as-per-rfc-2228)
  - [Account directories queryable](#account-directories-queryable)
  - [Membership provider changes](#membership-provider-changes)
- [Connection](#connection)
  - [Connection data changes](#connection-data-changes)
  - [Data connections](#data-connections)
  - [Connection checks](#connection-checks)
    - [Idle check](#idle-check)
    - [Connection check](#connection-check)
- [FTP middleware](#ftp-middleware)
  - [FTP request middleware](#ftp-request-middleware)
  - [FTP command execution middleware](#ftp-command-execution-middleware)
- [Server commands](#server-commands)
  - [`CloseConnectionServerCommand`](#closeconnectionservercommand)
  - [`SendResponseServerCommand`](#sendresponseservercommand)
- [FTP command execution](#ftp-command-execution)
  - [`FtpContext`](#ftpcontext)
  - [Command handlers (and attributes)](#command-handlers-and-attributes)
  - [Command extensions (and attributes)](#command-extensions-and-attributes)
  - [`FEAT` support](#feat-support)
- [Internals](#internals)
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
- Command handlers (and attributes)
- Command extensions (and attributes)
- `FEAT` support
- Connection
- FTP command collection

# File system changes

There are two important changes:

- [`IFileSystemClassFactory.Create`](xref:FubarDev.FtpServer.FileSystem.IFileSystemClassFactory.Create(FubarDev.FtpServer.IAccountInformation))
now requires an [`IAccountInformation`](xref:FubarDev.FtpServer.IAccountInformation) parameter
- The [`IUnixFileSystemEntry`](xref:FubarDev.FtpServer.FileSystem.IUnixFileSystemEntry) doesn't contain the `FileSystem` property anymore.

# Authorization/authentication as per RFC 2228

The authorization/authentication stack is new and implemented as
specified in the [RFC 2228](https://tools.ietf.org/rfc/rfc2228.txt).

This results in additional interfaces/extension points, like

- [`IAuthorizationMechanism`](xref:FubarDev.FtpServer.Authorization.IAuthorizationMechanism)
- [`IAuthenticationMechanism`](xref:FubarDev.FtpServer.Authentication.IAuthenticationMechanism)

There is also a new extension point for actions to be called when
the user is fully authorized: [`IAuthorizationAction`](xref:FubarDev.FtpServer.Authorization.IAuthorizationAction). You can develop
your own action, but you should only use an [`IAuthorizationAction.Level`](xref:FubarDev.FtpServer.Authorization.IAuthorizationAction.Level)
below 1000. The values from 1000 (incl.) to 2000 (incl.) are reserved by
the FTP server and are used to initialize the FTP connection data.

## Account directories queryable

A new interface has been introduced to get the root and home directories for
a given user.

Type name | Description
----------|----------------------
[`SingleRootWithoutHomeAccountDirectoryQuery`](xref:FubarDev.FtpServer.AccountManagement.Directories.SingleRootWithoutHome.SingleRootWithoutHomeAccountDirectoryQuery) | Provides a single root for all users.
[`RootPerUserAccountDirectoryQuery`](xref:FubarDev.FtpServer.AccountManagement.Directories.RootPerUser.RootPerUserAccountDirectoryQuery) | Gives every user its own root directory. Useful, when - for example - the file system root was set to `/home`.
`PamAccountDirectoryQuery` | Uses home directory information from PAM. The home directory can be configured to be the root instead.

## Membership provider changes

The membership provider is now asynchronous which means that the `ValidateUser` function was
renamed to [`ValidateUserAsync`](xref:FubarDev.FtpServer.AccountManagement.IMembershipProvider.ValidateUserAsync(System.String,System.String)).
Everything else is the same.

# Connection

The [`IFtpConnection`](xref:FubarDev.FtpServer.IFtpConnection) API was heavily overhauled to use a feature collection,
where the features can be queried through the [`Features`](xref:FubarDev.FtpServer.IFtpConnection.Features) property. Using the `WriteAsync`
function is obsolete. The FTP command handlers should use the `CommandContext`s
[`ServerCommandWriter`](xref:FubarDev.FtpServer.FtpContext.ServerCommandWriter)
if they need to send out-of-band responses.

Obsolete property | Target feature
------------------|----------------------------------
Encoding          | [`IEncodingFeature`](xref:FubarDev.FtpServer.Features.IEncodingFeature)
OriginalStream    | [`ISecureConnectionFeature`](xref:FubarDev.FtpServer.Features.ISecureConnectionFeature)
SocketStream      | [`ISecureConnectionFeature`](xref:FubarDev.FtpServer.Features.ISecureConnectionFeature)
IsSecure          | [`ISecureConnectionFeature`](xref:FubarDev.FtpServer.Features.ISecureConnectionFeature)

Obsolete method   | New home
------------------|--------------------------------------------------------
WriteAsync        | [`FtpCommandHandler.CommandContext.ResponseWriter`](xref:FubarDev.FtpServer.CommandHandlers.FtpCommandHandler.CommandContext) or [`FtpCommandHandlerExtension.CommandContext.ResponseWriter`](xref:FubarDev.FtpServer.CommandExtensions.FtpCommandHandlerExtension.CommandContext)

## Connection data changes

The whole [`FtpConnectionData`](xref:FubarDev.FtpServer.FtpConnectionData) class is marked as obsolete.

The connection datas `IsAnonymous` property is obsolete. An anonymous user is now detected by testing if
the [`FtpConnectionData.User`](xref:FubarDev.FtpServer.FtpConnectionData.User)
implements [`IAnonymousFtpUser`](xref:FubarDev.FtpServer.AccountManagement.IAnonymousFtpUser).

Most of the properties of [`IFtpConnection.Data`](xref:FubarDev.FtpServer.IFtpConnection.Data) were moved to a corresponding
feature.

Obsolete property       | Target feature
------------------------|----------------------------------
NlstEncoding            | [`IEncodingFeature`](xref:FubarDev.FtpServer.Features.IEncodingFeature)
User                    | [`IAuthorizationInformationFeature`](xref:FubarDev.FtpServer.Features.IAuthorizationInformationFeature)
FileSystem              | [`IFileSystemFeature`](xref:FubarDev.FtpServer.Features.IFileSystemFeature)
Path                    | [`IFileSystemFeature`](xref:FubarDev.FtpServer.Features.IFileSystemFeature)
CurrentDirectory        | [`IFileSystemFeature`](xref:FubarDev.FtpServer.Features.IFileSystemFeature)
Language                | [`ILocalizationFeature`](xref:FubarDev.FtpServer.Features.ILocalizationFeature)
Catalog                 | [`ILocalizationFeature`](xref:FubarDev.FtpServer.Features.ILocalizationFeature)
TransferMode            | [`ITransferConfigurationFeature`](xref:FubarDev.FtpServer.Features.ITransferConfigurationFeature)
PortAddress             | Removed
TransferTypeCommandUsed | Removed
RestartPosition         | [`IRestCommandFeature`](xref:FubarDev.FtpServer.Features.IRestCommandFeature)
RenameFrom              | [`IRenameCommandFeature`](xref:FubarDev.FtpServer.Features.IRenameCommandFeature)
ActiveMlstFacts         | [`IMlstFactsFeature`](xref:FubarDev.FtpServer.Features.IMlstFactsFeature)
PassiveSocketClient     | Removed
BackgroundCommandHandler| [`IBackgroundTaskLifetimeFeature`](xref:FubarDev.FtpServer.Features.IBackgroundTaskLifetimeFeature)
CreateEncryptedStream   | [`ISecureConnectionFeature`](xref:FubarDev.FtpServer.Features.ISecureConnectionFeature)


There's no direct replacement for the `UserData` property, but you can use the feature collection too.

## Data connections

We're now using two factories to create data connections:

- [`ActiveDataConnectionFeatureFactory`](xref:FubarDev.FtpServer.DataConnection.ActiveDataConnectionFeatureFactory) for active data connections (`PORT`/`EPRT` commands)
- [`PassiveDataConnectionFeatureFactory`](xref:FubarDev.FtpServer.DataConnection.PassiveDataConnectionFeatureFactory) for passive data connections (`PASV`/`EPSV` commands)

This factories create a [`IFtpDataConnectionFeature`](xref:FubarDev.FtpServer.Features.IFtpDataConnectionFeature) which is used to create [`IFtpDataConnection`](xref:FubarDev.FtpServer.IFtpDataConnection) implementations. This allows us to abstract away the differences between active and passive data connections.

The function `IFtpConnection.CreateResponseSocket` was replaced by [`DataConnectionServerCommand`](xref:FubarDev.FtpServer.ServerCommands.DataConnectionServerCommand) server command.
The passed [`AsyncDataConnectionDelegate`](xref:FubarDev.FtpServer.ServerCommands.AsyncDataConnectionDelegate) gets an
[`IFtpDataConnection`](xref:FubarDev.FtpServer.IFtpDataConnection) implementation and may return a response.
This function also takes care of SSL/TLS encryption as it wraps the [`IFtpDataConnection`](xref:FubarDev.FtpServer.IFtpDataConnection)
implementation returned by the [`IFtpDataConnectionFeature`](xref:FubarDev.FtpServer.Features.IFtpDataConnectionFeature) into
a new [`IFtpDataConnection`](xref:FubarDev.FtpServer.IFtpDataConnection) implementation with the help of
the [`SecureDataConnectionWrapper`](xref:FubarDev.FtpServer.DataConnection.SecureDataConnectionWrapper).

The extension method `SendResponseAsync` on the `IFtpConnection` was replaced by the [`DataConnectionServerCommand`](xref:FubarDev.FtpServer.ServerCommands.DataConnectionServerCommand)
server command and takes care of closing the [`IFtpDataConnection`](xref:FubarDev.FtpServer.IFtpDataConnection).

## Connection checks

The FTP server allows to check if a connection is still active. It ensures that the server doesn't get filled with dead connections, where
the server didn't recognize that the client is gone (e.g. client crash, aborted TCP connection, etc...).

This was made possible by using the following two implementations for [`IFtpConnectionCheck`](xref:FubarDev.FtpServer.ConnectionChecks.IFtpConnectionCheck):

- [`FtpConnectionEstablishedCheck`](xref:FubarDev.FtpServer.ConnectionChecks.FtpConnectionEstablishedCheck) checks if the TCP connection is still established
- [`FtpConnectionIdleCheck`](xref:FubarDev.FtpServer.ConnectionChecks.FtpConnectionIdleCheck) checks if the connection is idle for too long

This checks are enabled by default and can be disabled and reenabled by the following extension methods for
the [`IFtpServerBuilder`](xref:FubarDev.FtpServer.IFtpServerBuilder):

- [`DisableChecks`](xref:FubarDev.FtpServer.FtpServerBuilderExtensionsForChecks.DisableChecks*) disables all checks (the default ones and the ones manually enabled before)
- [`EnableDefaultChecks`](xref:FubarDev.FtpServer.FtpServerBuilderExtensionsForChecks.EnableDefaultChecks*) enables all default checks (see above)
- [`EnableIdleCheck`](xref:FubarDev.FtpServer.FtpServerBuilderExtensionsForChecks.EnableIdleCheck*) enables the check for an idle connection
- [`EnableConnectionCheck`](xref:FubarDev.FtpServer.FtpServerBuilderExtensionsForChecks.EnableConnectionCheck*) enables the check for an establised TCP connection
- [`DisableIdleCheck`](xref:FubarDev.FtpServer.FtpServerBuilderExtensionsForChecks.DisableIdleCheck*) enables the check for an idle connection
- [`DisableConnectionCheck`](xref:FubarDev.FtpServer.FtpServerBuilderExtensionsForChecks.DisableConnectionCheck*) disables the check for an establised TCP connection

The checks above are enabled by default.

### Idle check

The idle check determines if the connection was idle for too long. The default timeout is 5 minutes, configured through [`FtpConnectionOptions.InactivityTimeout`](xref:FubarDev.FtpServer.FtpConnectionOptions.InactivityTimeout).

### Connection check

This determines if the TCP connection is still established by sending an empty data packet to the client.

# FTP middleware

There are two types of middlewares:

- FTP request middleware (between FTP command collection and dispatch)
- FTP command execution middleware (between FTP command dispatch and execution)

## FTP request middleware

This middleware allows interception and modification of the received
FTP commands. You must implement and register the
[`IFtpMiddleware`](xref:FubarDev.FtpServer.IFtpMiddleware) interface as
service in your dependency injection container.

## FTP command execution middleware

The difference between this and the former middleware is, that the FTP command
handler for the FTP command is already selected and you can only intercept
the FTP commands or do something special.

You must implement and register the
[`IFtpCommandMiddleware`](xref:FubarDev.FtpServer.Commands.IFtpCommandMiddleware) interface as
service in your dependency injection container.

An example is the `FsIdChanger` in the `TestFtpServer` project. This middleware
sets - for every authenticated user - the UID/GID for file system access.

# Server commands

We're now supporting custom FTP server commands. Those commands must implement
[`IServerCommand`](xref:FubarDev.FtpServer.ServerCommands.IServerCommand) and
must have a corresponding handler ([`IServerCommandHandler<TCommand>`](xref:FubarDev.FtpServer.ServerCommands.IServerCommandHandler`1)).

## [`CloseConnectionServerCommand`](xref:FubarDev.FtpServer.ServerCommands.CloseConnectionServerCommand)

This command closes the FTP connection.

## [`SendResponseServerCommand`](xref:FubarDev.FtpServer.ServerCommands.SendResponseServerCommand)

This command sends a response to the client.

# FTP command execution

Massive changes were done to the FTP command execution. The center
of this changes is the new [`FtpContext`](xref:FubarDev.FtpServer.FtpContext)
which provides a new way to access all necessary information like
the FTP connection, the command information and a channel to send
server commands (which replaces `IFtpConnection.WriteAsync`).

## [`FtpContext`](xref:FubarDev.FtpServer.FtpContext)

The new [`FtpContext`](xref:FubarDev.FtpServer.FtpContext) is the FTP
servers equivalent of ASP.NET Core's `HttpContext` and provides access
to all information required to execute the FTP commands.

## Command handlers (and attributes)

The command handlers were overhauled in the following areas:

- Lazy initialization
  - Removed commands from DI container
    - You can still add your FTP command handlers to the DI container, but those may (most likely) be ignored from version 4.0 and up.
    - Implement your own [`IFtpCommandHandlerScanner`](xref:FubarDev.FtpServer.Commands.IFtpCommandHandlerScanner) or reuse [`AssemblyFtpCommandHandlerScanner`](xref:FubarDev.FtpServer.Commands.AssemblyFtpCommandHandlerScanner)
  - New [`IFtpCommandHandlerScanner`](xref:FubarDev.FtpServer.Commands.IFtpCommandHandlerScanner) which scans for types that may implement FTP command handlers
  - New [`IFtpCommandHandlerProvider`](xref:FubarDev.FtpServer.Commands.IFtpCommandHandlerProvider) which returns information for all found FTP command handler types
- Attributes for command information
  - [`FtpCommandHandlerAttribute`](xref:FubarDev.FtpServer.Commands.FtpCommandHandlerAttribute) which gives the FTP command handler a name and defines if it needs a successful login or if it's abortable
- Simplified constructor due to [`CommandContext`](xref:FubarDev.FtpServer.CommandHandlers.FtpCommandHandler.CommandContext) (type [`FtpCommandHandlerContext`](xref:FubarDev.FtpServer.FtpCommandHandlerContext)) property injection
- Activated (read: instantiated with property injection) by command name using the [`IFtpCommandActivator`](xref:FubarDev.FtpServer.Commands.IFtpCommandActivator) service

## Command extensions (and attributes)

The command extensions cannot be returned by `IFtpCommandHandler.GetExtensions()` anymore. The extensions were moved to
their own file and the default extensions are automatically registered as service.

- Lazy initialization
  - Removed command extensions from DI container
    - You can still add your FTP command handler extensions to the DI container, but those may (most likely) be ignored from version 4.0 and up.
    - Implement your own [`IFtpCommandHandlerExtensionScanner`](xref:FubarDev.FtpServer.CommandExtensions.IFtpCommandHandlerExtensionScanner) or reuse [`AssemblyFtpCommandHandlerExtensionScanner`](xref:FubarDev.FtpServer.CommandExtensions.AssemblyFtpCommandHandlerExtensionScanner)
  - New [`IFtpCommandHandlerExtensionScanner`](xref:FubarDev.FtpServer.CommandExtensions.IFtpCommandHandlerExtensionScanner) which scans for types that may implement FTP command handler extensions
  - New [`IFtpCommandHandlerExtensionProvider`](xref:FubarDev.FtpServer.CommandExtensions.IFtpCommandHandlerExtensionProvider) which returns information for all found FTP command handler extension types
- Attributes for command extension information
  - [`FtpCommandHandlerExtensionAttribute`](xref:FubarDev.FtpServer.CommandExtensions.FtpCommandHandlerExtensionAttribute) which gives the FTP command handler extension a name and defines the command it extends and if it needs a successful login
- Simplified constructor due to [`CommandContext`](xref:FubarDev.FtpServer.CommandExtensions.FtpCommandHandlerExtension.CommandContext) (type [`FtpCommandHandlerContext`](xref:FubarDev.FtpServer.FtpCommandHandlerContext)) property injection
- Automatic indirect activation (read: instantiation with property injection) for the command it belongs to through the [`IFtpCommandActivator`](xref:FubarDev.FtpServer.Commands.IFtpCommandActivator) service

## `FEAT` support

There are two new attributes to get the string to be returned by a `FEAT` command:

- [`FtpFeatureTextAttribute`](xref:FubarDev.FtpServer.FtpFeatureTextAttribute) contains the feature text itself
- [`FtpFeatureFunctionAttribute`](xref:FubarDev.FtpServer.FtpFeatureFunctionAttribute) contains the name of the static function to be called to get the feature text

# Internals

## FTP command collection changes

We're now using `ReadOnlySpan` for both [`TelnetInputParser`](xref:FubarDev.FtpServer.TelnetInputParser`1)
and [`FtpCommandCollector`](xref:FubarDev.FtpServer.FtpCommandCollector).

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
- New [`IFtpMiddleware`](xref:FubarDev.FtpServer.IFtpMiddleware) interface for custom request middleware
- New [`IFtpCommandMiddleware`](xref:FubarDev.FtpServer.Commands.IFtpCommandMiddleware) interface for custom command execution middleware
- New [FTP connection checks](#connection-checks)

## What's changed?

- Google drive upload without background uploader
- The `IFtpCommandHandler.GetExtensions()` is now deprecated as all extensions that were previously returned here have their own implementation now
- BREAKING: Usage of `ReadOnlySpan` in the FTP command collector
- BREAKING: [`IFileSystemClassFactory.Create`](xref:FubarDev.FtpServer.FileSystem.IFileSystemClassFactory.Create(FubarDev.FtpServer.IAccountInformation)) takes an [`IAccountInformation`](xref:FubarDev.FtpServer.IAccountInformation)
- BREAKING: The [`IMembershipProvider`](xref:FubarDev.FtpServer.AccountManagement.IMembershipProvider) is now asynchronous
- BREAKING: `FtpConnectionData.IsAnonymous` is obsolete, the anonymous user is now of type [`IAnonymousFtpUser`](xref:FubarDev.FtpServer.AccountManagement.IAnonymousFtpUser)
- BREAKING: Moved [`PromiscuousPasv`](xref:FubarDev.FtpServer.PasvCommandOptions.PromiscuousPasv) into [`PasvCommandOptions`](xref:FubarDev.FtpServer.PasvCommandOptions)
- BREAKING: Removed property `PortAddress`, `TransferTypeCommandUsed`, and `PassiveSocketClient` from `FtpConnectionData`, because we're using a new [`IFtpDataConnection`](xref:FubarDev.FtpServer.IFtpDataConnection) abstraction

## What's fixed?

- AUTH TLS fails gracefully when no SSL certificate is configured
- `SITE BLST` works again
- Fixed deadlock in [`MultiBindingTcpListener`](xref:FubarDev.FtpServer.MultiBindingTcpListener)
- Thread safe increment/decrement for connection counter (fixes [#68](https://github.com/FubarDevelopment/FtpServer/issues/68))
- The `.` directory will be returned again (fixes [#56](https://github.com/FubarDevelopment/FtpServer/issues/56))

# A look into the future

The 4.x version will drop support for .NET Standard 1.3 and - most likely - .NET 4.6.1 as
the FTP Server will be reimplemented as `ConnectionHandler` which will result into the following
improvements:

- Easy hosting in an ASP.NET Core application
- Using the ASP.NET Core connection state management
