//-----------------------------------------------------------------------
// <copyright file="SystCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <code>SYST</code> command.
    /// </summary>
    public class SystCommandHandler : FtpCommandHandler
    {
        private readonly string _operatingSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="SystCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection to create this command handler for.</param>
        /// <param name="options">Options for the SYST command.</param>
        public SystCommandHandler(IFtpConnection connection, IOptions<SystCommandOptions> options)
            : base(connection, "SYST")
        {
            _operatingSystem = options.Value.OperatingSystem ?? "UNIX";
        }

        /// <inheritdoc/>
        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            return Task.FromResult(new FtpResponse(200, $"{_operatingSystem} Type: {Connection.Data.TransferMode}"));
        }
    }
}
