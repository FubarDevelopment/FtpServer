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

namespace FubarDev.FtpServer.CommandHandlers
{
    public class OptsCommandHandler : FtpCommandHandler
    {
        private static readonly char[] _whiteSpaces = { ' ', '\t' };

        public OptsCommandHandler(FtpConnection connection)
            : base(connection, "OPTS")
        {
            SupportedExtensions = new List<string>
            {
                "UTF8",
            };
        }

        public override IReadOnlyCollection<string> SupportedExtensions { get; }

        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var argument = command.Argument;
            var whiteSpaceIndex = argument.IndexOfAny(_whiteSpaces);
            var commandName = whiteSpaceIndex == -1 ? argument : argument.Substring(0, whiteSpaceIndex);
            var commandArgument = whiteSpaceIndex == -1 ? string.Empty : argument.Substring(whiteSpaceIndex + 1);

            switch (commandName.ToUpperInvariant())
            {
                case "UTF8":
                case "UTF-8":
                    return await ProcessOptionUtf8(commandArgument);
                default:
                    return await Task.FromResult(new FtpResponse(500, "Syntax error, command unrecognized."));
            }
        }

        private Task<FtpResponse> ProcessOptionUtf8(string commandArgument)
        {
            switch (commandArgument.ToUpperInvariant())
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
