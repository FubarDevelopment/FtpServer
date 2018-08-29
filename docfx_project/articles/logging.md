---
uid: logging
title: FTP Server logging
---

# Introduction

The FTP server utilizes [`Microsoft.Extensions.Logging`](https://docs.microsoft.com/aspnet/core/fundamentals/logging) which provides an interface to many logging frameworks (e.g. [NLog](https://github.com/NLog/NLog/wiki)).

# Example: Using SeriLog

## Adding SeriLog to the project

Go to the quickstart project created during the [Quickstart](xref:quickstart) tutorial and add the following NuGet packages:

```bash
# Serilog.Extensions.Logging
dotnet add package Serilog.Extensions.Logging
# Serilog.Sinks.Console
dotnet add package Serilog.Sinks.Console
```

## Configure serilog in Program.cs

Add the highlighted lines to your Program.cs:

[!code-cs[Program.cs](../code-snippets/logging-serilog/Program.cs?highlight=9-10,17-23,27-29 "The FTP server")]

Now you can see all the log messages from the FTP server.

# Example: Using NLog

## Adding NLog to the project

Go to the quickstart project created during the [Quickstart](xref:quickstart) tutorial and add the following NuGet package:

```bash
# Add NLog
dotnet add package NLog.Extensions.Logging
```

## Add the NLog configuration

Now add a file called `nlog.config` with the following contents:

[!code-xml[nlog.config](../code-snippets/logging-nlog/nlog.config "NLog configuration")]

## Add the configuration to the project

Change the `csproj` file by adding the following lines:

[!code-xml[QuickStart.csproj](../code-snippets/logging-nlog/QuickStart.csproj?highlight=13-18 "Project file")]

This ensures that the `nlog.config` file gets copied into the build output folder and is available for the application.

## Registering NLog in Program.cs

[!code-cs[Program.cs](../code-snippets/logging-nlog/Program.cs?highlight=8-10,21-23,41-46 "The FTP server")]

Now you can see all the log messages from the FTP server.
