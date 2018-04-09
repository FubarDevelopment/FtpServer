// <copyright file="ServiceCollectionExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.CommandExtensions;
using FubarDev.FtpServer.FileSystem;

using Microsoft.Extensions.DependencyInjection;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Extension methods for <see cref="IServiceCollection"/>
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the FTP server services to the collection
        /// </summary>
        /// <typeparam name="TFileSystemFactory">The file system factory implementation</typeparam>
        /// <typeparam name="TMembershipProvider">The membership provider implementation</typeparam>
        /// <param name="services">The service collection to add the FTP server services to</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddFtpServer<TFileSystemFactory, TMembershipProvider>(this IServiceCollection services)
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
