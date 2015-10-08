using System.IO;
using System.Threading.Tasks;

using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.FileSystem.DotNet;

namespace TestFtpServer.FileSystem
{
    public class DotNetFileSystemProvider : IFileSystemClassFactory
    {
        private readonly string _rootPath;

        public DotNetFileSystemProvider(string rootPath)
        {
            _rootPath = rootPath;
        }

        public Task<IUnixFileSystem> Create(string userId)
        {
            return Task.FromResult<IUnixFileSystem>(new DotNetFileSystem(Path.Combine(_rootPath, userId)));
        }
    }
}
