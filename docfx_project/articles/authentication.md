---
uid: authentication
title: Authentication
---

# Introduction

The FTP server currently supports the following membership providers:

- `AnonymousMembershipProvider` is the only portable membership provider in the project
- `PamMembershipProvider` requres .NET Core 3.0 and a system with an installed libpam.so.0

# How it works

The FTP server queries all services implementing the [`IMembershipProvider`](xref:FubarDev.FtpServer.AccountManagement.IMembershipProvider) interface and tries to authenticate the given login/password against all registered providers. The first provider that was able to authenticate the user is stored in the FTP connection data.

> [!WARNING]
> The order of the registration of the membership providers is important.

# Anonymous authentication

The anonymous authentiation is implemented by the [`AnonymousMembershipProvider`](xref:FubarDev.FtpServer.AccountManagement.AnonymousMembershipProvider) class and can be configured by adding a service for an [`IAnonymousPasswordValidator`](xref:FubarDev.FtpServer.AccountManagement.Anonymous.IAnonymousPasswordValidator).

## Default anonymous password validators

- [NoValidation](xref:FubarDev.FtpServer.AccountManagement.Anonymous.NoValidation): Take the password without validation
- [NoTopLevelDomainValidation](xref:FubarDev.FtpServer.AccountManagement.Anonymous.NoTopLevelDomainValidation): The E-Mail-Address doesn't need to have a TLD
- [SimpleMailAddressValidation](xref:FubarDev.FtpServer.AccountManagement.Anonymous.SimpleMailAddressValidation): Simple E-Mail validation
- [BlockAnonymousValidation](xref:FubarDev.FtpServer.AccountManagement.Anonymous.BlockAnonymousValidation): Block anonymous logins

The `SimpleMailAddressValidation` is the default password validator.
