---
uid: file-systems
title: File Systems
---

# Overview

The FTP server provides two file systems by default:

- `System.IO`-based file system
- Google Drive-based file system

The `System.IO`-based file system is usually what you want and the Google Drive-based file system is a proof-of-concept.

## `System.IO`-based file system

This file system just uses the operating systems underlying file system and serves it to the user.

### Configuration

Here is an example of a configuration:

```csharp
services.Configure<DotNetFileSystemOptions>(cfg => {
    cfg.RootPath = "/your/root/path";
});
```

You can also configure the following:

- Usage of the user ID as subdirectory
- Is deletion of non-empty directories allowed?

## Google Drive-based file system

This topic is explained in a [separate article](xref:google-drive).

## Your own file system

This topic is explained in a [separate article](xref:custom-file-system).
