//-----------------------------------------------------------------------
// <copyright file="OptsCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.CommandExtensions;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <code>OPTS</code> command.
    /// </summary>
    public class OptsCommandHandler : FtpCommandHandler, IFtpCommandHandlerExtensionHost
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OptsCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection to create this command handler for</param>
        public OptsCommandHandler(FtpConnection connection)
            : base(connection, "OPTS")
        {
            Extensions = new Dictionary<string, FtpCommandHandlerExtension>(StringComparer.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public IDictionary<string, FtpCommandHandlerExtension> Extensions { get; }

        /// <inheritdoc/>
        public override IEnumerable<IFeatureInfo> GetSupportedFeatures()
        {
            yield return new GenericFeatureInfo("UTF8", "UTF-8");
        }

        /// <inheritdoc/>
        public override IEnumerable<FtpCommandHandlerExtension> GetExtensions()
        {
            yield return new GenericFtpCommandHandlerExtension(Connection, "OPTS", "UTF8", ProcessOptionUtf8, "UTF-8");
        }

        /// <inheritdoc/>
        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var argument = FtpCommand.Parse(command.Argument);
            FtpCommandHandlerExtension extension;
            if (!Extensions.TryGetValue(argument.Name, out extension))
                return new FtpResponse(500, "Syntax error, command unrecognized.");

            return await extension.Process(argument, cancellationToken);
        }

        private Task<FtpResponse> ProcessOptionUtf8(FtpCommand command, CancellationToken cancellationToken)
        {
            switch (command.Argument.ToUpperInvariant())
            {
                case "ON":
                    Connection.Encoding = Encoding.UTF8;
                    return Task.FromResult(new FtpResponse(200, "Command okay."));
                case "":
                    Connection.Data.NlstEncoding = null;
                    return Task.FromResult(new FtpResponse(200, "Command okay."));
                case "NLST":
                    Connection.Data.NlstEncoding = Encoding.UTF8;
                    return Task.FromResult(new FtpResponse(200, "Command okay."));
                default:
                    return Task.FromResult(new FtpResponse(501, "Syntax error in parameters or arguments."));
            }
        }
    }
}
