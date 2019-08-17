//-----------------------------------------------------------------------
// <copyright file="SystCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.Features;

using Microsoft.Extensions.Options;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>SYST</c> command.
    /// </summary>
    [FtpCommandHandler("SYST", isLoginRequired: false)]
    public class SystCommandHandler : FtpCommandHandler
    {
        private readonly string _operatingSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystCommandHandler"/> class.
        /// </summary>
        /// <param name="options">Options for the SYST command.</param>
        public SystCommandHandler(IOptions<SystCommandOptions> options)
        {
            _operatingSystem = options.Value.OperatingSystem ?? "UNIX";
        }

        /// <inheritdoc />
        [Obsolete("Information about an FTP command handler can be queried through the IFtpCommandHandlerProvider service.")]
        public override bool IsLoginRequired => false;

        /// <inheritdoc/>
        public override Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var transferMode = Connection.Features.Get<ITransferConfigurationFeature>().TransferMode;
            return Task.FromResult<IFtpResponse?>(new FtpResponse(215, T("{0} Type: {1}", _operatingSystem, transferMode)));
        }
    }
}
