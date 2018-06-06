---
uid: ftps
title: FTP over SSL/TLS
---

# Introduction

The FTPS support enables encrypted communication with the server.

# How to use FTPS

FTPS is automatically enabled as soon as you have set an X509 certificate with private key (read: the `X509Certificate2` from a `.pfx`/`.pkcs12` file) in the [AuthTlsOptions](xref:FubarDev.FtpServer.AuthTlsOptions).

# Example

```csharp
var cert = new X509Certificate2("my.pfx", "my-super-strong-password-that-nobody-knows");
services.Configure<AuthTlsOptions>(cfg => cfg.ServerCertificate = cert);
```

# Epilogue

You can test this - like many other things - using the sample application in the repository.
