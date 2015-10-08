using System.IO;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.FileSystem.DotNet
{
    public class DotNetSystemProvider : IFileSystemClassFactory
    {
        private readonly string _rootPath;

        public DotNetSystemProvider(string rootPath)
        {
            _rootPath = rootPath;
        }

        public Task<IUnixFileSystem> Create(string userId)
        {
            return Task.FromResult<IUnixFileSystem>(new DotNetFileSystem(Path.Combine(_rootPath, userId)));
        }
    }
}
