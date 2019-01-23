//-----------------------------------------------------------------------
// <copyright file="TypeCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>TYPE</c> command.
    /// </summary>
    public class TypeCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeCommandHandler"/> class.
        /// </summary>
        /// <param name="connectionAccessor">The accessor to get the connection that is active during the <see cref="Process"/> method execution.</param>
        public TypeCommandHandler(IFtpConnectionAccessor connectionAccessor)
            : base(connectionAccessor, "TYPE")
        {
        }

        /// <inheritdoc/>
        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var transferMode = FtpTransferMode.Parse(command.Argument);

            FtpResponse response;
            if (transferMode.FileType == FtpFileType.Ascii)
            {
                response = new FtpResponse(200, "ASCII transfer mode active.");
            }
            else if (transferMode.IsBinary)
            {
                response = new FtpResponse(200, "Binary transfer mode active.");
            }
            else
            {
                response = new FtpResponse(504, $"Mode {command.Argument} not supported.");
            }

            if (response.Code == 200)
            {
                Connection.Data.TransferMode = transferMode;
            }

            return Task.FromResult(response);
        }
    }
}
