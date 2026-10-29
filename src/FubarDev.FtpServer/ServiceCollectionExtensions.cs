// <copyright file="ServiceCollectionExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Reflection;

using FubarDev.FtpServer;
using FubarDev.FtpServer.AccountManagement.Directories.SingleRootWithoutHome;
using FubarDev.FtpServer.Authentication;
using FubarDev.FtpServer.Authorization;
using FubarDev.FtpServer.BackgroundTransfer;
using FubarDev.FtpServer.CommandExtensions;
using FubarDev.FtpServer.CommandHandlers;
using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.DataConnection;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.Localization;
using FubarDev.FtpServer.Networking;
using FubarDev.FtpServer.ServerCommandHandlers;
using FubarDev.FtpServer.ServerCommands;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the FTP server services to the collection.
        /// </summary>
        /// <param name="services">The service collection to add the FTP server services to.</param>
        /// <param name="configure">Configuration of the FTP server services.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddFtpServer(
            this IServiceCollection services,
            Action<IFtpServerBuilder> configure)
        {
            services.AddOptions();

            services.AddSingleton<IFtpServer, FtpServer>();
            services.AddSingleton<IFtpListenerService, DefaultFtpListenerService>();
            services.AddSingleton<ITemporaryDataFactory, TemporaryDataFactory>();
            services.AddSingleton<IPasvListenerFactory, PasvListenerFactory>();
            services.AddSingleton<IPasvAddressResolver, SimplePasvAddressResolver>();
            services.AddSingleton<IFtpConnectionAccessor, FtpConnectionAccessor>();

            var commandAssembly = typeof(PassCommandHandler).GetTypeInfo().Assembly;

            // Command handlers
            services.AddSingleton<IFtpCommandHandlerScanner>(
                _ => new AssemblyFtpCommandHandlerScanner(commandAssembly));
#pragma warning disable CS0612 // Typ oder Element ist veraltet
            services.AddScoped<IFtpCommandHandlerScanner, ServiceFtpCommandHandlerScanner>();
#pragma warning restore CS0612 // Typ oder Element ist veraltet
            services.TryAddScoped<IFtpCommandHandlerProvider, DefaultFtpCommandHandlerProvider>();

            // Command handler extensions
            services.AddScoped<IFtpCommandHandlerExtensionScanner>(
                sp => new AssemblyFtpCommandHandlerExtensionScanner(
                    sp.GetRequiredService<IFtpCommandHandlerProvider>(),
                    sp.GetService<ILogger<AssemblyFtpCommandHandlerExtensionScanner>>(),
                    commandAssembly));
#pragma warning disable CS0612 // Typ oder Element ist veraltet
            services.AddScoped<IFtpCommandHandlerExtensionScanner, ServiceFtpCommandHandlerExtensionScanner>();
#pragma warning restore CS0612 // Typ oder Element ist veraltet
            services.TryAddScoped<IFtpCommandHandlerExtensionProvider, DefaultFtpCommandHandlerExtensionProvider>();

            // Activator for FTP commands (and extensions)
            services.AddScoped<IFtpCommandActivator, DefaultFtpCommandActivator>();

            // Feature provider
            services.AddScoped<IFeatureInfoProvider, DefaultFeatureInfoProvider>();

            services.AddScoped<TcpSocketClientAccessor>();
            services.AddScoped(sp => sp.GetRequiredService<TcpSocketClientAccessor>().TcpSocketClient ?? throw new NullReferenceException("TCP socket client null"));

            services.AddScoped<IFtpConnection, FtpConnection>();
            services.AddScoped<IFtpLoginStateMachine, FtpLoginStateMachine>();
#pragma warning disable 618
            services.AddScoped<IBackgroundCommandHandler>(sp => new BackgroundCommandHandler(sp.GetRequiredService<IFtpConnection>()));
#pragma warning restore 618
            services.AddScoped<IFtpCommandDispatcher, DefaultFtpCommandDispatcher>();

            services.AddScoped<IFtpHostSelector, SingleFtpHostSelector>();

            services.AddSingleton<IFtpCatalogLoader, DefaultFtpCatalogLoader>();
            services.TryAddSingleton<IFtpServerMessages, DefaultFtpServerMessages>();

            services.AddSingleton<IBackgroundTransferWorker, BackgroundTransferWorker>();

            services.AddSingleton(sp => (IFtpService)sp.GetRequiredService<IFtpServer>());
            services.AddSingleton(sp => (IFtpService)sp.GetRequiredService<IBackgroundTransferWorker>());

            services.AddSingleton<IFtpServerHost, FtpServerHost>();

            services.AddSingleton<ISslStreamWrapperFactory, DefaultSslStreamWrapperFactory>();

            services.TryAddSingleton<IAccountDirectoryQuery, SingleRootWithoutHomeAccountDirectoryQuery>();

            services.Scan(
                sel => sel.FromAssemblyOf<IAuthorizationAction>()
                   .AddClasses(filter => filter.AssignableTo<IAuthorizationAction>()).As<IAuthorizationAction>().WithSingletonLifetime());

            services.Scan(
                sel => sel.FromAssemblyOf<PasswordAuthorization>()
                   .AddClasses(filter => filter.AssignableTo<IAuthorizationMechanism>()).As<IAuthorizationMechanism>().WithScopedLifetime());

            services.Scan(
                sel => sel.FromAssemblyOf<TlsAuthenticationMechanism>()
                   .AddClasses(filter => filter.AssignableTo<IAuthenticationMechanism>()).As<IAuthenticationMechanism>().WithScopedLifetime());

            services
               .AddScoped<IServerCommandExecutor, ReflectionServerCommandExecutor>();

            services
               .AddScoped<IServerCommandHandler<SendResponseServerCommand>, SendResponseServerCommandHandler>()
               .AddScoped<IServerCommandHandler<CloseConnectionServerCommand>, CloseConnectionServerCommandHandler>()
               .AddScoped<IServerCommandHandler<TlsEnableServerCommand>, TlsEnableServerCommandHandler>()
               .AddScoped<IServerCommandHandler<PauseConnectionServerCommand>, PauseConnectionServerCommandHandler>()
               .AddScoped<IServerCommandHandler<ResumeConnectionServerCommand>, ResumeConnectionServerCommandHandler>()
               .AddScoped<IServerCommandHandler<DataConnectionServerCommand>, DataConnectionServerCommandHandler>()
               .AddScoped<IServerCommandHandler<CloseDataConnectionServerCommand>, CloseDataConnectionServerCommandHandler>();

            services
               .AddSingleton<ActiveDataConnectionFeatureFactory>()
               .AddSingleton<PassiveDataConnectionFeatureFactory>()
               .AddSingleton<SecureDataConnectionWrapper>();

            services
               .AddSingleton<IFtpDataConnectionValidator, PromiscuousPasvDataConnectionValidator>();

            configure(new FtpServerBuilder(services).EnableDefaultChecks());

            return services;
        }

        private class FtpServerBuilder : IFtpServerBuilder
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="FtpServerBuilder"/> class.
            /// </summary>
            /// <param name="services">The service collection.</param>
            public FtpServerBuilder(IServiceCollection services)
            {
                Services = services;
            }

            /// <inheritdoc />
            public IServiceCollection Services { get; }
        }
    }
}
