// <copyright file="Program.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

using FubarDev.FtpServer;
using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.AccountManagement.Directories.RootPerUser;
using FubarDev.FtpServer.AccountManagement.Directories.SingleRootWithoutHome;
using FubarDev.FtpServer.Authentication;
using FubarDev.FtpServer.CommandExtensions;
using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.FileSystem.DotNet;
using FubarDev.FtpServer.FileSystem.GoogleDrive;
using FubarDev.FtpServer.FileSystem.InMemory;
using FubarDev.FtpServer.FileSystem.Unix;
using FubarDev.FtpServer.MembershipProvider.Pam;
using FubarDev.FtpServer.MembershipProvider.Pam.Directories;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Mono.Options;

using NLog.Extensions.Logging;

using TestFtpServer.Commands;
using TestFtpServer.Extensions;
using TestFtpServer.Utilities;

namespace TestFtpServer
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            var options = new TestFtpServerOptions();

            var optionSet = new CommandSet("ftpserver")
            {
                "usage: ftpserver [OPTIONS] <COMMAND> [COMMAND-OPTIONS]",
                { "h|?|help", "Show help", v => options.ShowHelp = v != null },
                "Authentication",
                { "authentication=", "Sets the authentication (custom, anonymous)", v =>
                    {
                        switch (v)
                        {
                            case "custom":
                                options.MembershipProviderType |= MembershipProviderType.Custom;
                                break;
                            case "anonymous":
                                options.MembershipProviderType |= MembershipProviderType.Anonymous;
                                break;
                            case "pam":
                                options.MembershipProviderType |= MembershipProviderType.PAM;
                                break;
                            default:
                                throw new ApplicationException("Invalid authentication module");
                        }
                    }
                },
                "PAM authentication workarounds",
                { "no-pam-account-management", "Disable the PAM account management", v => options.NoPamAccountManagement = v != null },
                "Directory layout (filesystem, unix))",
                { "l|layout=", "Directory layout", v =>                     {
                        switch (v)
                        {
                            case "default":
                            case "root":
                            case "single":
                            case "single-root":
                                options.DirectoryLayout = DirectoryLayout.SingleRoot;
                                break;
                            case "root-per-user":
                            case "per-user-root":
                            case "per-user":
                                options.DirectoryLayout = DirectoryLayout.RootPerUser;
                                break;
                            case "pam":
                            case "passwd":
                            case "pam-home":
                            case "passwd-home":
                                options.DirectoryLayout = DirectoryLayout.PamHomeDirectory;
                                break;
                            case "pam-root":
                            case "passwd-root":
                                options.DirectoryLayout = DirectoryLayout.PamHomeDirectoryAsRoot;
                                break;
                            default:
                                throw new ApplicationException("Invalid authentication module");
                        }
                    }
                },
                "Server",
                { "a|address=", "Sets the IP address or host name", v => options.ServerAddress = v },
                { "p|port=", "Sets the listen port", v => options.Port = Convert.ToInt32(v) },
                { "s|pasv=", "Sets the range for PASV ports, specify as FIRST:LAST", v =>
                    {
                        var sPorts = v.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                        if (sPorts.Length != 2)
                        {
                            throw new ApplicationException("Need exactly two ports for PASV port range");
                        }

                        var iPorts = sPorts.Select(s => Convert.ToInt32(s)).ToArray();

                        if (iPorts[1] < iPorts[0])
                        {
                            throw new ApplicationException("PASV start port must be smaller than end port");
                        }

                        options.PassivePortRange = (iPorts[0], iPorts[1]) ;
                    }
                },
                { "promiscuous", "Allows promiscuous PASV", v => options.PromiscuousPasv = v != null },
                "FTPS",
                { "c|certificate=", "Set the SSL certificate", v => options.ServerCertificateFile = v },
                { "P|password=", "Password for the SSL certificate", v => options.ServerCertificatePassword = v },
                { "i|implicit", "Use implicit FTPS", v => options.ImplicitFtps = XmlConvert.ToBoolean(v.ToLowerInvariant()) },
                "Backends",
                new Command("filesystem", "Use the System.IO file system access")
                {
                    Options = new OptionSet()
                    {
                        "usage: ftpserver filesystem [ROOT-DIRECTORY]",
                    },
                    Run = a => RunWithFileSystemAsync(a.ToArray(), options).Wait(),
                },
                new Command("unix", "Use the Unix file system access")
                {
                    Options = new OptionSet()
                    {
                        "usage: ftpserver unix",
                        { "no-id-change", "Don't change the effective user id for file operations", v => options.DisableUserIdSwitch = v != null }
                    },
                    Run = a => RunWithUnixFileSystemAsync(options).Wait(),
                },
                new Command("in-memory", "Use the in-memory file system access")
                {
                    Options = new OptionSet()
                    {
                        "usage: ftpserver in-memory [OPTIONS]",
                        { "keep-anonymous", "Keep anonymous in-memory file systems", v => options.KeepAnonymousInMemoryFileSystem = v != null }
                    },
                    Run = a => RunWithInMemoryFileSystemAsync(options).Wait(),
                },
                new CommandSet("google-drive")
                {
                    { "b|background|background-upload", "Use background upload", v => options.UseBackgroundUpload = v != null },
                    new Command("user", "Use a users Google Drive as file system")
                    {
                        Options = new OptionSet()
                        {
                            "usage: ftpserver google-drive user <CLIENT-SECRETS-FILE> <USERNAME>",
                            { "r|refresh", "Refresh the access token", v => options.RefreshToken = v != null },
                        },
                        Run = a => RunWithGoogleDriveUserAsync(a.ToArray(), options).Wait(),
                    },
                    new Command("service", "Use a users Google Drive with a service account")
                    {
                        Options = new OptionSet()
                        {
                            "usage: ftpserver google-drive service <SERVICE-CREDENTIAL-FILE>",
                        },
                        Run = a => RunWithGoogleDriveServiceAsync(a.ToArray(), options).Wait(),
                    },
                },
            };

            return optionSet.Run(args);
        }

        private static Task RunWithInMemoryFileSystemAsync(TestFtpServerOptions options)
        {
            options.Validate();
            var services = CreateServices(options)
               .Configure<InMemoryFileSystemOptions>(
                    opt => opt.KeepAnonymousFileSystem = options.KeepAnonymousInMemoryFileSystem)
               .AddFtpServer(sb => Configure(sb, options).UseInMemoryFileSystem());
            return RunAsync(services);
        }

        private static Task RunWithFileSystemAsync(string[] args, TestFtpServerOptions options)
        {
            options.Validate();
            var rootDir =
                args.Length != 0 ? args[0] : Path.Combine(Path.GetTempPath(), "TestFtpServer");
            var services = CreateServices(options)
               .Configure<DotNetFileSystemOptions>(opt => opt.RootPath = rootDir)
               .AddFtpServer(sb => Configure(sb, options).UseDotNetFileSystem());
            return RunAsync(services);
        }

        private static Task RunWithUnixFileSystemAsync(TestFtpServerOptions options)
        {
            options.Validate();
            var services = CreateServices(options)
               .Configure<UnixFileSystemOptions>(opt => opt.DisableUserIdSwitch = options.DisableUserIdSwitch)
               .AddFtpServer(sb => Configure(sb, options).UseUnixFileSystem());
            return RunAsync(services);
        }

        private static async Task RunWithGoogleDriveUserAsync(string[] args, TestFtpServerOptions options)
        {
            options.Validate();
            if (args.Length != 2)
            {
                throw new Exception("This command requires two arguments: <CLIENT-SECRETS-FILE> <USERNAME>");
            }

            var clientSecretsFile = args[0];
            var userName = args[1];

            UserCredential credential;
            using (var secretsSource = new FileStream(clientSecretsFile, FileMode.Open))
            {
                var secrets = GoogleClientSecrets.Load(secretsSource);
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    secrets.Secrets,
                    new[] { DriveService.Scope.DriveFile, DriveService.Scope.Drive },
                    userName,
                    CancellationToken.None);
                if (options.RefreshToken)
                {
                    await credential.RefreshTokenAsync(CancellationToken.None);
                }
            }

            var services = CreateServices(options)
               .AddFtpServer(sb => Configure(sb, options).UseGoogleDrive(credential));
            await RunAsync(services).ConfigureAwait(false);
        }

        private static Task RunWithGoogleDriveServiceAsync(string[] args, TestFtpServerOptions options)
        {
            options.Validate();
            if (args.Length != 1)
            {
                throw new Exception("This command requires one argument: <SERVICE-CREDENTIAL-FILE>");
            }

            var serviceCredentialFile = args[0];
            var credential = GoogleCredential
                .FromFile(serviceCredentialFile)
                .CreateScoped(DriveService.Scope.Drive, DriveService.Scope.DriveFile);

            var services = CreateServices(options)
               .AddFtpServer(sb => Configure(sb, options).UseGoogleDrive(credential));
            return RunAsync(services);
        }

        private static async Task RunAsync(IServiceCollection services)
        {
            using (var serviceProvider = services.BuildServiceProvider())
            {
                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                loggerFactory.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });
                NLog.LogManager.LoadConfiguration("NLog.config");

                try
                {
                    // Start the FTP server
                    var ftpServerHost = serviceProvider.GetRequiredService<IFtpServerHost>();
                    await ftpServerHost.StartAsync(CancellationToken.None).ConfigureAwait(false);

                    Console.WriteLine("Press ENTER/RETURN to close the test application.");
                    Console.ReadLine();

                    // Stop the FTP server
                    await ftpServerHost.StopAsync(CancellationToken.None).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            }
        }

        private static IServiceCollection CreateServices(TestFtpServerOptions options)
        {
            var services = new ServiceCollection()
               .AddLogging(cfg => cfg.SetMinimumLevel(LogLevel.Trace))
               .AddOptions()
               .Configure<AuthTlsOptions>(
                    opt =>
                    {
                        if (options.ServerCertificateFile != null)
                        {
                            opt.ServerCertificate = new X509Certificate2(
                                options.ServerCertificateFile,
                                options.ServerCertificatePassword);
                        }
                    })
               .Configure<FtpConnectionOptions>(opt => opt.DefaultEncoding = Encoding.ASCII)
               .Configure<FtpServerOptions>(
                    opt =>
                    {
                        opt.ServerAddress = options.ServerAddress;
                        opt.Port = options.GetPort();
                    })
               .Configure<SimplePasvOptions>(
                    opt =>
                    {
                        if (options.PassivePortRange != null)
                        {
                            opt.PasvMinPort = options.PassivePortRange.Value.Item1;
                            opt.PasvMaxPort = options.PassivePortRange.Value.Item2;
                        }
                    })
               .Configure<PasvCommandOptions>(opt => opt.PromiscuousPasv = options.PromiscuousPasv)
               .Configure<GoogleDriveOptions>(opt => opt.UseBackgroundUpload = options.UseBackgroundUpload)
               .Configure<PamMembershipProviderOptions>(opt => opt.IgnoreAccountManagement = options.NoPamAccountManagement);

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

            switch (options.DirectoryLayout)
            {
                case DirectoryLayout.SingleRoot:
                    services.AddSingleton<IAccountDirectoryQuery, SingleRootWithoutHomeAccountDirectoryQuery>();
                    break;
                case DirectoryLayout.RootPerUser:
                    services
                       .AddSingleton<IAccountDirectoryQuery, RootPerUserAccountDirectoryQuery>()
                       .Configure<RootPerUserAccountDirectoryQueryOptions>(opt => opt.AnonymousRootPerEmail = true);
                    break;
                case DirectoryLayout.PamHomeDirectory:
                    services
                       .AddSingleton<IAccountDirectoryQuery, PamAccountDirectoryQuery>()
                       .Configure<PamAccountDirectoryQueryOptions>(opt => opt.AnonymousRootDirectory = "/tmp");
                    break;
                case DirectoryLayout.PamHomeDirectoryAsRoot:
                    services
                       .AddSingleton<IAccountDirectoryQuery, PamAccountDirectoryQuery>()
                       .Configure<PamAccountDirectoryQueryOptions>(opt =>
                        {
                            opt.AnonymousRootDirectory = "/tmp";
                            opt.UserHomeIsRoot = true;
                        });
                    break;
            }

            if (options.ImplicitFtps)
            {
                services.Decorate<IFtpServer>(
                    (ftpServer, serviceProvider) =>
                    {
                        var authTlsOptions = serviceProvider.GetRequiredService<IOptions<AuthTlsOptions>>();
                        var sslStreamWrapperFactory = serviceProvider.GetRequiredService<ISslStreamWrapperFactory>();

                        // Use an implicit SSL connection (without the AUTHTLS command)
                        ftpServer.ConfigureConnection += (s, e) =>
                        {
                            var secureConnectionFeature = e.Connection.Features.Get<ISecureConnectionFeature>();
                            var sslStream = sslStreamWrapperFactory.WrapStreamAsync(
                                    secureConnectionFeature.OriginalStream,
                                    false,
                                    authTlsOptions.Value.ServerCertificate,
                                    CancellationToken.None)
                               .Result;
                            secureConnectionFeature.SocketStream = sslStream;
                        };

                        return ftpServer;
                    });
            }

            return services;
        }

        private static IFtpServerBuilder Configure(IFtpServerBuilder builder, TestFtpServerOptions options)
        {
            if (options.MembershipProviderType == MembershipProviderType.Default)
            {
                return builder.EnableAnonymousAuthentication();
            }

            if ((options.MembershipProviderType & MembershipProviderType.Anonymous) != 0)
            {
                builder = builder.EnableAnonymousAuthentication();
            }

            if ((options.MembershipProviderType & MembershipProviderType.Custom) != 0)
            {
                builder.Services.AddSingleton<IMembershipProvider, CustomMembershipProvider>();
            }

            if ((options.MembershipProviderType & MembershipProviderType.PAM) != 0)
            {
                builder = builder.EnablePamAuthentication();
            }

            return builder;
        }
    }
}
