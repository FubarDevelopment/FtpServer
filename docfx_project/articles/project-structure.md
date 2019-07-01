---
uid: project-structure
title: FTP Server Project Structure
---

# Introduction

The FTP server consists of the following parts:

- Account management
  - Membership providers (e.g. for anonymous users, PAM, etc...)
  - Member information (e.g. home directories, etc...)
- Authentication
  - Authentication mechanisms (e.g. `AUTH TLS`)
- Authorization
  - Authorization mechanisms (e.g. password)
  - Authorization actions (e.g. for setting the home directory)
- Commands (e.g. `LIST`)
- File systems
  - .NET I/O
  - Google Drive
  - Unix
  - In-memory

# Account management

The account management is centered around the following interfaces:

- `IMembershipProvider` for username/password authentication
- `IAccountDirectoryQuery` to get the users root and home directories

The `IMembershipProvider` is only useful for password-based authorization
mechanisms.

# Authentication

There is currently only one authentication mechanism implemented: `TLS`.

# Authorization

Only one authorization mechanism is implemented: `PasswordAuthorization`.

Every authorization mechanism is centered around three commands:

- `USER`: Provides the user name
- `PASS`: Provides the password
- `ACCT`: Provides additional information

The full state machine for an authentication/authorization can
be found in RFC 2228.

# Commands

All commands are implemented as thread-safe singletons. This is
a requirement due to the fact that commands can provide extensions
that will show up during a `FEAT` request.

There are two kinds of extensions:

- `FEAT`-Extensions (provided by a FTP command handler)
- Extensions hosted by an FTP command, like `OPTS` or `SITE`

The `FEAT` extensions are only there to tell the client
that some commands support special features.

Extensions can also be attached to a command that is also an
extension host, like `OPTS` or `SITE`. Those extensions are
reported to the `FEAT` command through the extension hosts.

# File systems

File systems are discussed [here](xref:file-systems)
