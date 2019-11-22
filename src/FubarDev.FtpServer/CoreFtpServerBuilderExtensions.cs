// <copyright file="CoreFtpServerBuilderExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Authentication;

using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Core extensions for <see cref="IFtpServerBuilder"/>.
    /// </summary>
    public static class CoreFtpServerBuilderExtensions
    {
        public static IFtpServerBuilder UseImplicitTls(this IFtpServerBuilder builder, X509Certificate certificate)
        {
            builder.Services
               .AddSingleton(new ImplicitFtpsControlConnectionStreamAdapterOptions(certificate))
               .AddSingleton<IFtpControlStreamAdapter, ImplicitFtpsControlConnectionStreamAdapter>()
               .AddSingleton<IFtpConnectionConfigurator, AuthTlsConfigurator>()
               .Configure<AuthTlsOptions>(
                    options =>
                    {
                        options.ServerCertificate = certificate;
                        options.ImplicitFtps = true;
                    });
            return builder;
        }

        private class ImplicitFtpsControlConnectionStreamAdapterOptions
        {
            public ImplicitFtpsControlConnectionStreamAdapterOptions(X509Certificate certificate)
            {
                Certificate = certificate;
            }

            public X509Certificate Certificate { get; }
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
                return _sslStreamWrapperFactory
                   .WrapStreamAsync(stream, false, _options.Certificate, cancellationToken);
            }
        }

        private class AuthTlsConfigurator : IFtpConnectionConfigurator
        {
            /// <inheritdoc />
            public Task Configure(IFtpConnection connection, CancellationToken cancellationToken)
            {
                var serviceProvider = connection.Features.Get<IServiceProvidersFeature>().RequestServices;
                var stateMachine = serviceProvider.GetRequiredService<IFtpLoginStateMachine>();
                var authTlsMechanism = serviceProvider.GetRequiredService<IEnumerable<IAuthenticationMechanism>>()
                   .Single(x => x.CanHandle("TLS"));
                stateMachine.Activate(authTlsMechanism);
                return Task.CompletedTask;
            }
        }
    }
}
