// <copyright file="StatCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.ListFormatters;

using Minimatch;

namespace FubarDev.FtpServer.CommandHandlers
{
    public class StatCommandHandler : FtpCommandHandler
    {
        public StatCommandHandler(FtpConnection connection)
            : base(connection, "STAT")
        {
        }

        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(command.Argument))
            {
                var taskStates = Server.GetBackgroundTaskStates();
                var statusMessage = new StringBuilder();
                statusMessage.AppendFormat("Server functional, {0} open connections", Server.Statistics.ActiveConnections);
                if (taskStates.Count != 0)
                    statusMessage.AppendFormat(", {0} active background transfers", taskStates.Count);
                return new FtpResponse(211, statusMessage.ToString());
            }

            var mask = command.Argument;
            if (!mask.EndsWith("*"))
                mask += "*";

            var mmOptions = new Options()
            {
                IgnoreCase = Data.FileSystem.FileSystemEntryComparer.Equals("a", "A"),
                NoGlobStar = true,
                Dot = true,
            };

            var mm = new Minimatcher(mask, mmOptions);

            var formatter = new LongListFormatter();
            await Connection.Write($"211-STAT {command.Argument}", cancellationToken);

            foreach (var entry in (await Data.FileSystem.GetEntriesAsync(Data.CurrentDirectory, cancellationToken)).Where(x => mm.IsMatch(x.Name)))
            {
                var line = formatter.Format(entry);
                Connection.Log?.Debug(line);
                await Connection.Write($" {line}", cancellationToken);
            }

            return new FtpResponse(211, "STAT");
        }
    }
}
