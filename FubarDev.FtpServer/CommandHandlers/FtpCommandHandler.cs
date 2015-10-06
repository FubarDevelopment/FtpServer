//-----------------------------------------------------------------------
// <copyright file="FtpCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer.CommandHandlers
{
    public abstract class FtpCommandHandler
    {
        protected FtpCommandHandler(FtpConnection connection, string name, params string[] alternativeNames)
        {
            Connection = connection;
            var names = new List<string>
            {
                name
            };
            names.AddRange(alternativeNames);
            Names = names;
        }

        public IReadOnlyCollection<string> Names { get; }

        public virtual IReadOnlyCollection<string> SupportedExtensions => null;

        public virtual bool IsLoginRequired => false;

        public virtual bool IsAbortable => false;

        protected FtpConnection Connection { get; }

        protected FtpServer Server => Connection.Server;

        protected FtpConnectionData Data => Connection.Data;

        public abstract Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken);
    }
}
