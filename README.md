# Portable FTP server

This FTP server is written as PCL and has an abstract file system
which allows e.g. Google Drive as backend.

# License

The library is released under the [MIT license](http://opensource.org/licenses/MIT).

# Example FTP server

```csharp
// allow only anonymous logins
var membershipProvider = new AnonymousMembershipProvider();

// use %TEMP%/TestFtpServer as root folder
var fsProvider = new DotNetFileSystemProvider(Path.Combine(Path.GetTempPath(), "TestFtpServer"), false);

// Initialize the FTP server
var ftpServer = new FtpServer(fsProvider, membershipProvider, "127.0.0.1");

Console.WriteLine("Press ENTER/RETURN to close the test application.");

// Start the FTP server
ftpServer.Start();

Console.ReadLine();

// Stop the FTP server
ftpServer.Stop();
```
