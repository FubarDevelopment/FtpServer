using System;
using System.IO;
using System.Text;

using FubarDev.FtpServer;
using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.AccountManagement.Anonymous;
using FubarDev.FtpServer.FileSystem.DotNet;

using TestFtpServer.Logging;

namespace TestFtpServer
{
    class Program
    {
        private static void Main()
        {
            var membershipProvider = new AnonymousMembershipProvider(new NoValidation());
            var fsProvider = new DotNetFileSystemProvider(Path.Combine(Path.GetTempPath(), "TestFtpServer"));
            using (var ftpServer = new FtpServer(fsProvider, membershipProvider, "127.0.0.1")
            {
                DefaultEncoding = Encoding.ASCII,
                LogManager = new FtpLogManager(),
            })
            {
                Console.WriteLine("Press ENTER/RETURN to close the test application.");
                var log = ftpServer.LogManager?.CreateLog(typeof(Program));

                try
                {
                    ftpServer.Start();
                    Console.ReadLine();
                    ftpServer.Stop();
                }
                catch (Exception ex)
                {
                    log?.Error(ex, "Error during main FTP server loop");
                }
            }
        }
    }
}
