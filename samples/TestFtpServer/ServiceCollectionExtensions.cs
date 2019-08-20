// <copyright file="ServiceCollectionExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer;
using FubarDev.FtpServer.AccountManagement.Directories.RootPerUser;
using FubarDev.FtpServer.AccountManagement.Directories.SingleRootWithoutHome;
using FubarDev.FtpServer.Authentication;
using FubarDev.FtpServer.CommandExtensions;
using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.FileSystem.DotNet;
using FubarDev.FtpServer.FileSystem.GoogleDrive;
using FubarDev.FtpServer.FileSystem.InMemory;
using FubarDev.FtpServer.FileSystem.Unix;
using FubarDev.FtpServer.MembershipProvider.Pam;
using FubarDev.FtpServer.MembershipProvider.Pam.Directories;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;

using Microsoft.DotNet.PlatformAbstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Mono.Unix.Native;

using TestFtpServer.CommandMiddlewares;
using TestFtpServer.Commands;
using TestFtpServer.Configuration;
using TestFtpServer.Extensions;
using TestFtpServer.ServerInfo;
using TestFtpServer.Utilities;

namespace TestFtpServer
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddFtpServices(
            this IServiceCollection services,
            FtpOptions options)
        {
            services
               .Configure<AuthTlsOptions>(
                    opt =>
                    {
                        opt.ServerCertificate = options.GetCertificate();
                        opt.ImplicitFtps = options.Ftps.Implicit;
                    })
               .Configure<FtpConnectionOptions>(opt => opt.DefaultEncoding = Encoding.ASCII)
               .Configure<FubarDev.FtpServer.FtpServerOptions>(
                    opt =>
                    {
                        opt.ServerAddress = options.Server.Address;
                        opt.Port = options.GetServerPort();
                        opt.MaxActiveConnections = options.Server.MaxActiveConnections ?? 0;
                    })
               .Configure<PortCommandOptions>(
                    opt =>
                    {
                        if (options.Server.UseFtpDataPort)
                        {
                            opt.DataPort = options.GetServerPort() - 1;
                        }
                    })
               .Configure<SimplePasvOptions>(
                    opt =>
                    {
                        var portRange = options.GetPasvPortRange();
                        if (portRange != null)
                        {
                            (opt.PasvMinPort, opt.PasvMaxPort) = portRange.Value;
                        }
                    })
               .Configure<PasvCommandOptions>(opt => opt.PromiscuousPasv = options.Server.Pasv.Promiscuous)
               .Configure<GoogleDriveOptions>(opt => opt.UseBackgroundUpload = options.GoogleDrive.BackgroundUpload)
               .Configure<PamMembershipProviderOptions>(
                    opt => opt.IgnoreAccountManagement = options.Pam.NoAccountManagement);

            // Add "Hello" service - unique per FTP connection
            services.AddScoped<Hello>();

            // Add custom command handlers
            services.AddSingleton<IFtpCommandHandlerScanner>(
                _ => new AssemblyFtpCommandHandlerScanner(typeof(HelloFtpCommandHandler).Assembly));

            // Add custom command handler extensions
            services.AddSingleton<IFtpCommandHandlerExtensionScanner>(
                sp => new AssemblyFtpCommandHandlerExtensionScanner(
                    sp.GetRequiredService<IFtpCommandHandlerProvider>(),
                    sp.GetService<ILogger<AssemblyFtpCommandHandlerExtensionScanner>>(),
                    typeof(SiteHelloFtpCommandHandlerExtension).Assembly));

            if (options.SetFileSystemId && RuntimeEnvironment.OperatingSystemPlatform !=
                Microsoft.DotNet.PlatformAbstractions.Platform.Windows)
            {
                services.AddScoped<IFtpCommandMiddleware, FsIdChanger>();
            }

            switch (options.BackendType)
            {
                case FileSystemType.InMemory:
                    services = services
                       .AddFtpServer(sb => sb.ConfigureAuthentication(options).UseInMemoryFileSystem())
                       .Configure<InMemoryFileSystemOptions>(
                            opt => opt.KeepAnonymousFileSystem = options.InMemory.KeepAnonymous);
                    break;
                case FileSystemType.SystemIO:
                    services = services
                       .AddFtpServer(sb => sb.ConfigureAuthentication(options).UseDotNetFileSystem())
                       .Configure<DotNetFileSystemOptions>(opt => opt.RootPath = options.SystemIo.Root);
                    break;
                case FileSystemType.Unix:
                    services = services
                       .AddFtpServer(sb => sb.ConfigureAuthentication(options).UseUnixFileSystem())
                       .Configure<UnixFileSystemOptions>(opt => opt.Root = options.Unix.Root);
                    break;
                case FileSystemType.GoogleDriveUser:
                    var userCredential = GetUserCredential(
                            options.GoogleDrive.User.ClientSecrets ?? throw new ArgumentNullException(
                                nameof(options.GoogleDrive.User.ClientSecrets),
                                "Client secrets file not specified."),
                            options.GoogleDrive.User.UserName ?? throw new ArgumentNullException(
                                nameof(options.GoogleDrive.User.ClientSecrets),
                                "User name not specified."),
                            options.GoogleDrive.User.RefreshToken);
                    services = services
                       .AddFtpServer(sb => sb.ConfigureAuthentication(options).UseGoogleDrive(userCredential));
                    break;
                case FileSystemType.GoogleDriveService:
                    var serviceCredential = GoogleCredential
                       .FromFile(options.GoogleDrive.Service.CredentialFile)
                       .CreateScoped(DriveService.Scope.Drive, DriveService.Scope.DriveFile);
                    services = services
                       .AddFtpServer(sb => sb.ConfigureAuthentication(options).UseGoogleDrive(serviceCredential));
                    break;
                default:
                    throw new NotSupportedException(
                        $"Backend of type {options.Backend} cannot be run from configuration file options.");
            }

            switch (options.LayoutType)
            {
                case FileSystemLayoutType.SingleRoot:
                    services.AddSingleton<IAccountDirectoryQuery, SingleRootWithoutHomeAccountDirectoryQuery>();
                    break;
                case FileSystemLayoutType.RootPerUser:
                    services
                       .AddSingleton<IAccountDirectoryQuery, RootPerUserAccountDirectoryQuery>()
                       .Configure<RootPerUserAccountDirectoryQueryOptions>(opt => opt.AnonymousRootPerEmail = true);
                    break;
                case FileSystemLayoutType.PamHome:
                    services
                       .AddSingleton<IAccountDirectoryQuery, PamAccountDirectoryQuery>()
                       .Configure<PamAccountDirectoryQueryOptions>(
                            opt => opt.AnonymousRootDirectory = Path.GetTempPath());
                    break;
                case FileSystemLayoutType.PamHomeChroot:
                    services
                       .AddSingleton<IAccountDirectoryQuery, PamAccountDirectoryQuery>()
                       .Configure<PamAccountDirectoryQueryOptions>(
                            opt =>
                            {
                                opt.AnonymousRootDirectory = Path.GetTempPath();
                                opt.UserHomeIsRoot = true;
                            });
                    break;
            }

            if (options.Ftps.Implicit)
            {
                var implicitFtpsCertificate = options.GetCertificate();
                if (implicitFtpsCertificate != null)
                {
                    services
                       .AddSingleton(new ImplicitFtpsControlConnectionStreamAdapterOptions(implicitFtpsCertificate))
                       .AddSingleton<IFtpControlStreamAdapter, ImplicitFtpsControlConnectionStreamAdapter>();

                    // Ensure that PROT and PBSZ commands are working.
                    services.Decorate<IFtpServer>(
                        (ftpServer, _) =>
                        {
                            ftpServer.ConfigureConnection += (s, e) =>
                            {
                                var serviceProvider = e.Connection.ConnectionServices;
                                var stateMachine = serviceProvider.GetRequiredService<IFtpLoginStateMachine>();
                                var authTlsMechanism = serviceProvider.GetRequiredService<IEnumerable<IAuthenticationMechanism>>()
                                   .Single(x => x.CanHandle("TLS"));
                                stateMachine.Activate(authTlsMechanism);
                            };

                            return ftpServer;
                        });
                }
            }

            services.Decorate<IFtpServer>(
                (ftpServer, serviceProvider) =>
                {
                    /* Setting the umask is only valid for non-Windows platforms. */
                    if (!string.IsNullOrEmpty(options.Umask)
                        && RuntimeEnvironment.OperatingSystemPlatform !=
                        Microsoft.DotNet.PlatformAbstractions.Platform.Windows)
                    {
                        var umask = options.Umask!.StartsWith("0")
                            ? Convert.ToInt32(options.Umask, 8)
                            : Convert.ToInt32(options.Umask, 10);

                        Syscall.umask((FilePermissions)umask);
                    }

                    return ftpServer;
                });

            services.Scan(
                ts => ts
                   .FromAssemblyOf<HostedFtpService>()
                   .AddClasses(itf => itf.AssignableTo<IModuleInfo>(), true).As<IModuleInfo>()
                   .WithSingletonLifetime());

            return services;
        }

        private static UserCredential GetUserCredential(
            string clientSecretsFile,
            string userName,
            bool refreshToken)
        {
            UserCredential credential;
            using (var secretsSource = new FileStream(clientSecretsFile, FileMode.Open))
            {
                var secrets = GoogleClientSecrets.Load(secretsSource);
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        secrets.Secrets,
                        new[] { DriveService.Scope.DriveFile, DriveService.Scope.Drive },
                        userName,
                        CancellationToken.None).Result;
            }

            if (refreshToken)
            {
                credential.RefreshTokenAsync(CancellationToken.None).Wait();
            }

            return credential;
        }

        private class ImplicitFtpsControlConnectionStreamAdapterOptions
        {
            public ImplicitFtpsControlConnectionStreamAdapterOptions(X509Certificate2 certificate)
            {
                Certificate = certificate;
            }

            public X509Certificate2 Certificate { get; }
        }

        private class ImplicitFtpsControlConnectionStreamAdapter : IFtpControlStreamAdapter
        {
            private readonly ImplicitFtpsControlConnectionStreamAdapterOptions _options;
            private readonly ISslStreamWrapperFactory _sslStreamWrapperFactory;

            public ImplicitFtpsControlConnectionStreamAdapter(
                ImplicitFtpsControlConnectionStreamAdapterOptions options,
                ISslStreamWrapperFactory sslStreamWrapperFactory)
            {
                _options = options;
                _sslStreamWrapperFactory = sslStreamWrapperFactory;
            }

            /// <inheritdoc />
            public Task<Stream> WrapAsync(Stream stream, CancellationToken cancellationToken)
            {
                return _sslStreamWrapperFactory.WrapStreamAsync(stream, false, _options.Certificate, cancellationToken);
            }
        }
    }
}
