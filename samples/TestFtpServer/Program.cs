// <copyright file="Program.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

using FubarDev.FtpServer;
using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.FileSystem.DotNet;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Mono.Options;

using NLog.Extensions.Logging;

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
                                options.MembershipProviderType = MembershipProviderType.Custom;
                                break;
                            case "anonymous":
                                options.MembershipProviderType = MembershipProviderType.Anonymous;
                                break;
                            default:
                                throw new ApplicationException("Invalid authentication module");
                        }
                    }
                },
                "Server",
                { "a|address=", "Sets the IP address or host name", v => options.ServerAddress = v },
                { "p|port=", "Sets the listen port", v => options.Port = Convert.ToInt32(v) },
                "FTPS",
                { "c|certificate=", "Set the SSL certificate", v => options.ServerCertificateFile =v },
                { "P|password=", "Password for the SSL certificate", v => options.ServerCertificatePassword = v },
                { "i|implicit", "Use implicit FTPS", v => options.ImplicitFtps = XmlConvert.ToBoolean(v.ToLowerInvariant()) },
                "Backends",
                new Command("filesystem", "Use the System.IO file system access")
                {
                    Options = new OptionSet()
                    {
                        "usage: ftpserver filesystem [ROOT-DIRECTORY]",
                    },
                    Run = a => RunWithFileSystem(a.ToArray(), options),
                },
                new CommandSet("google-drive")
                {
                    new Command("user", "Use a users Google Drive as file system")
                    {
                        Options = new OptionSet()
                        {
                            "usage: ftpserver google-drive user <CLIENT-SECRETS-FILE> <USERNAME>",
                            { "r|refresh", "Refresh the access token", v => options.RefreshToken = v != null },
                        },
                        Run = a => RunWithGoogleDriveUser(a.ToArray(), options).Wait(),
                    },
                    new Command("service", "Use a users Google Drive with a service account")
                    {
                        Options = new OptionSet()
                        {
                            "usage: ftpserver google-drive service <SERVICE-CREDENTIAL-FILE>",
                        },
                        Run = a => RunWithGoogleDriveService(a.ToArray(), options),
                    },
                },
            };

            return optionSet.Run(args);
        }

        private static void RunWithFileSystem(string[] args, TestFtpServerOptions options)
        {
            options.Validate();
            var rootDir =
                args.Length != 0 ? args[0] : Path.Combine(Path.GetTempPath(), "TestFtpServer");
            var services = CreateServices(options)
                .Configure<DotNetFileSystemOptions>(opt => opt.RootPath = rootDir)
                .AddFtpServer(sb => Configure(sb, options).UseDotNetFileSystem());
            Run(services, options);
        }

        private static async Task RunWithGoogleDriveUser(string[] args, TestFtpServerOptions options)
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
            Run(services, options);
        }

        private static void RunWithGoogleDriveService(string[] args, TestFtpServerOptions options)
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
            Run(services, options);
        }

        private static void Run(IServiceCollection services, TestFtpServerOptions options)
        {
            using (var serviceProvider = services.BuildServiceProvider())
            {
                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                loggerFactory.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });
                NLog.LogManager.LoadConfiguration("NLog.config");

                var ftpServer = serviceProvider.GetRequiredService<IFtpServer>();

                if (options.ImplicitFtps)
                {
                    var authTlsOptions = serviceProvider.GetRequiredService<IOptions<AuthTlsOptions>>();
                    // Use an implicit SSL connection (without the AUTHTLS command)
                    ftpServer.ConfigureConnection += (s, e) =>
                    {
                        var sslStream = new SslStream(e.Connection.OriginalStream);
                        sslStream.AuthenticateAsServer(authTlsOptions.Value.ServerCertificate);
                        e.Connection.SocketStream = sslStream;
                    };
                }

                try
                {
                    // Start the FTP server
                    ftpServer.Start();
                    Console.WriteLine("Press ENTER/RETURN to close the test application.");
                    Console.ReadLine();

                    // Stop the FTP server
                    ftpServer.Stop();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }
            }
        }

        private static IServiceCollection CreateServices(TestFtpServerOptions options)
        {
            var services = new ServiceCollection();
            services.AddLogging(cfg => cfg.SetMinimumLevel(LogLevel.Trace));
            services.AddOptions();

            services.Configure<AuthTlsOptions>(opt =>
            {
                if (options.ServerCertificateFile != null)
                {
                    opt.ServerCertificate = new X509Certificate2(
                        options.ServerCertificateFile,
                        options.ServerCertificatePassword);
                }
            });

            services.Configure<FtpConnectionOptions>(opt => opt.DefaultEncoding = Encoding.ASCII);
            services.Configure<FtpServerOptions>(opt =>
            {
                opt.ServerAddress = options.ServerAddress;
                opt.Port = options.GetPort();
            });

            return services;
        }

        private static IFtpServerBuilder Configure(IFtpServerBuilder builder, TestFtpServerOptions options)
        {
            switch (options.MembershipProviderType)
            {
                case MembershipProviderType.Anonymous:
                    return builder.EnableAnonymousAuthentication();
                case MembershipProviderType.Custom:
                    builder.Services.AddSingleton<IMembershipProvider, CustomMembershipProvider>();
                    break;
                default:
                    throw new InvalidOperationException($"Unknown membership provider {options.MembershipProviderType}");
            }

            return builder;
        }
    }
}
