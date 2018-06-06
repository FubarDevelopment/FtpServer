---
uid: custom-file-system
title: Custom File System Development
---

> [!WARNING]
> This guide is under construction!

# Overview

This article will show you how to develop your own file system.

> [!WARNING]
> This is a very advanced topic requiring solid knowledge of .NET.

# Interfaces

The following interfaces must be implemented:

- `IFileSystemClassFactory` is used to get user-specific file system access
- `IUnixFileSystem` provides the main file system operations
- `IUnixDirectoryEntry` represents a directory entry
- `IUnixFileEntry` represents a file entry

