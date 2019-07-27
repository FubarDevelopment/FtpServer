// <copyright file="HelloFtpCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer;
using FubarDev.FtpServer.CommandHandlers;
using FubarDev.FtpServer.Commands;

using TestFtpServer.Utilities;

namespace TestFtpServer.Commands
{
    /// <summary>
    /// The <c>HELLO</c> FTP command.
    /// </summary>
    [FtpCommandHandler("HELLO")]
    [FtpFeatureText("HELLO")]
    public class HelloFtpCommandHandler : FtpCommandHandler
    {
        private readonly Hello _hello;

        /// <summary>
        /// Initializes a new instance of the <see cref="HelloFtpCommandHandler"/> class.
        /// </summary>
        /// <param name="hello">The greeter.</param>
        public HelloFtpCommandHandler(Hello hello)
        {
            _hello = hello;
        }

        /// <inheritdoc />
        public override Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult<IFtpResponse?>(_hello.CreateResponse(command.Argument));
        }
    }
}
