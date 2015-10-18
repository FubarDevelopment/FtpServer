//-----------------------------------------------------------------------
// <copyright file="OptsCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <code>OPTS</code> command.
    /// </summary>
    public class OptsCommandHandler : FtpCommandHandler
    {
        private static readonly char[] _whiteSpaces = { ' ', '\t' };

        /// <summary>
        /// Initializes a new instance of the <see cref="OptsCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection to create this command handler for</param>
        public OptsCommandHandler(FtpConnection connection)
            : base(connection, "OPTS")
        {
        }

        /// <inheritdoc/>
        public override IEnumerable<IFeatureInfo> GetSupportedExtensions()
        {
            yield return new GenericFeatureInfo("UTF8", ProcessOptionUtf8, null, "UTF-8");
        }

        /// <inheritdoc/>
        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var argument = command.Argument ?? string.Empty;
            var whiteSpaceIndex = argument.IndexOfAny(_whiteSpaces);
            var commandName = whiteSpaceIndex == -1 ? argument : argument.Substring(0, whiteSpaceIndex);
            var commandArgument = whiteSpaceIndex == -1 ? string.Empty : argument.Substring(whiteSpaceIndex + 1);

            var commandHandlers = Connection
                .CommandHandlers.Values.Distinct().SelectMany(x => x.GetSupportedExtensions()).Where(x => x.Handler != null);
            var cmdHandlers2 = commandHandlers
                .SelectMany(x => x.Names, (x, s) => new { Name = s, x.Handler })
                .ToList();
            var cmdHandlers = cmdHandlers2
                .ToDictionary(x => x.Name, x => x.Handler, StringComparer.OrdinalIgnoreCase);

            FeatureHandlerDelgate handler;
            if (!cmdHandlers.TryGetValue(commandName, out handler))
                return new FtpResponse(500, "Syntax error, command unrecognized.");

            return await handler(Connection, commandArgument);
        }

        private static Task<FtpResponse> ProcessOptionUtf8(FtpConnection connection, string commandArgument)
        {
            switch (commandArgument.ToUpperInvariant())
            {
                case "ON":
                    connection.Encoding = Encoding.UTF8;
                    return Task.FromResult(new FtpResponse(200, "Command okay."));
                case "":
                    connection.Data.NlstEncoding = null;
                    return Task.FromResult(new FtpResponse(200, "Command okay."));
                case "NLST":
                    connection.Data.NlstEncoding = Encoding.UTF8;
                    return Task.FromResult(new FtpResponse(200, "Command okay."));
                default:
                    return Task.FromResult(new FtpResponse(501, "Syntax error in parameters or arguments."));
            }
        }
    }
}
