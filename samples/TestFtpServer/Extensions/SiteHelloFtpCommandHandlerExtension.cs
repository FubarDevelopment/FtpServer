// <copyright file="SiteHelloFtpCommandHandlerExtension.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer;
using FubarDev.FtpServer.CommandExtensions;

using TestFtpServer.Utilities;

namespace TestFtpServer.Extensions
{
    [FtpCommandHandlerExtension("HELLO", "SITE")]
    [FtpFeatureText("SITE HELLO")]
    public class SiteHelloFtpCommandHandlerExtension : FtpCommandHandlerExtension
    {
        private readonly Hello _hello;

        /// <summary>
        /// Initializes a new instance of the <see cref="SiteHelloFtpCommandHandlerExtension"/> class.
        /// </summary>
        /// <param name="hello">The greeter.</param>
        public SiteHelloFtpCommandHandlerExtension(Hello hello)
        {
            _hello = hello;
        }

        /// <inheritdoc />
        public override void InitializeConnectionData()
        {
        }

        /// <inheritdoc />
        public override Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult<IFtpResponse?>(_hello.CreateResponse(command.Argument));
        }
    }
}
