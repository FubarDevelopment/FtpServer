---
uid: google-drive
title: Gooogle Drive-based virtual file system
---

# Introduction

This is an example of a virtual file system that uses Google Drive as backend.

> [!IMPORTANT]
> It is a basic requirement that you know how to use the
> Google developer console to register your application
> and get the necessary credentials/information.

The Google Drive support uses [Googles .NET API](https://github.com/google/google-api-dotnet-client) which is
an advantage for people already familiar with it.

# Adding the package to your project

```bash
dotnet add package FubarDev.FtpServer.FileSystem.GoogleDrive
```

## Using a users drive

This is what other applications do when they want to access your Google Drive and the easiest to configure/use.

## Getting the client secret

1. Open the [Google Developer Console](https://console.developers.google.com)
2. Create a project
3. Allow the project to use the Google Drive API
4. Create an OAuth 2.0 client (Other)
5. Download the client secrets file

The client secrets file should look like this:

```json
{
  "installed": {
    "client_id": "redacted.apps.googleusercontent.com",
    "project_id": "your-project-id-123456",
    "auth_uri": "https://accounts.google.com/o/oauth2/auth",
    "token_uri": "https://accounts.google.com/o/oauth2/token",
    "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
    "client_secret": "redacted",
    "redirect_uris": [
      "urn:ietf:wg:oauth:2.0:oob",
      "http://localhost"
    ]
  }
}
```

## Configuring Google Drive as virtual file system

```csharp
// Configuration
var userName = "your-user-name@gmail.com";
var clientSecretsFile = "client_secret_redacted.apps.googleusercontent.com.json";

// Loading the credentials
UserCredential credential;
using (var secretsSource = new System.IO.FileStream(clientSecretsFile, FileMode.Open))
{
    var secrets = GoogleClientSecrets.Load(secretsSource);
    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
        secrets.Secrets,
        new[] { DriveService.Scope.DriveFile, DriveService.Scope.Drive },
        userName,
        CancellationToken.None);
}

// Adding the FTP server using Google Drive
services
    .AddFtpServer(sb => sb
        .UseGoogleDrive(credential)
        .EnableAnonymousAuthentication());
```

## Starting the server

Upon start, the FTP server loads the credentials and - if not already authenticated - opens the web page to authenticate your server to access the users Google Drive.
