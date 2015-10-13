using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer;
using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.AccountManagement.Anonymous;
using FubarDev.FtpServer.FileSystem.DotNet;

using TestFtpServer.Logging;

namespace TestFtpServer
{
    class Program
    {
        private static readonly CancellationTokenSource _tokenSrc = new CancellationTokenSource();

        private static void Main()
        {
            var membershipProvider = new AnonymousMembershipProvider(new NoValidation());
            var fsProvider = new DotNetFileSystemProvider(Path.Combine(Path.GetTempPath(), "TestFtpServer"));
            var server = new FtpServer(fsProvider, membershipProvider, "127.0.0.1")
            {
                DefaultEncoding = Encoding.ASCII,
                LogManager = new FtpLogManager(),
            };
            Console.WriteLine("Press ENTER/RETURN to close the test application.");
            Task.Run(() => Run(server));
            Console.ReadLine();
            _tokenSrc.Cancel();
        }

        private static void Run(FtpServer ftpServer)
        {
            var log = ftpServer.LogManager?.CreateLog(typeof(Program));

            try
            {
                ftpServer.Start();
                do
                {
                }
                while (!_tokenSrc.Token.WaitHandle.WaitOne(200));
                ftpServer.Stop();
            }
            catch (Exception ex)
            {
                log?.Error(ex, "Error during main FTP server loop");
            }
            finally
            {
                ftpServer.Dispose();
            }
        }
    }
}
