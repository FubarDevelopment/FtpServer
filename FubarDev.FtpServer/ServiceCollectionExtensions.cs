using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.CommandExtensions;
using FubarDev.FtpServer.FileSystem;

using Microsoft.Extensions.DependencyInjection;

namespace FubarDev.FtpServer
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFtpServer<TFileSystemFactory,TMembershipProvider>(this IServiceCollection services)
            where TFileSystemFactory : class, IFileSystemClassFactory
            where TMembershipProvider : class, IMembershipProvider
        {
            services.AddSingleton<IFileSystemClassFactory, TFileSystemFactory>();
            services.AddSingleton<IMembershipProvider, TMembershipProvider>();
            
            services.AddSingleton<FtpServer>();

            services.AddScoped<IFtpConnectionAccessor, FtpConnectionAccessor>();
            services.AddScoped<TcpSocketClientAccessor>();
            services.AddScoped(sp => sp.GetRequiredService<TcpSocketClientAccessor>().TcpSocketClient);

            services.AddScoped<IFtpConnection, FtpConnection>();

            services.Scan(
                sel => sel.FromAssemblyOf<FtpServer>()
                    .AddClasses(filter => filter.AssignableTo<IFtpCommandHandler>()).As<IFtpCommandHandler>().WithScopedLifetime()
                    .AddClasses(filter => filter.AssignableTo<IFtpCommandHandlerExtension>().Where(t => t != typeof(GenericFtpCommandHandlerExtension))).As<IFtpCommandHandlerExtension>().WithScopedLifetime());

            return services;
        }
    }
}
