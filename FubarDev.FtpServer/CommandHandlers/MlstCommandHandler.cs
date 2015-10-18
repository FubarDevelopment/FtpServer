// <copyright file="MlstCommandHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.CommandExtensions;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.ListFormatters;

using Sockets.Plugin.Abstractions;

namespace FubarDev.FtpServer.CommandHandlers
{
    public class MlstCommandHandler : FtpCommandHandler
    {
        private static readonly ISet<string> _knownFacts = new HashSet<string> { "type", "size", "perm", "modify" };

        public MlstCommandHandler(FtpConnection connection)
            : base(connection, "MLST", "MLSD")
        {
            connection.Data.ActiveMlstFacts.Clear();
            foreach (var knownFact in _knownFacts)
                connection.Data.ActiveMlstFacts.Add(knownFact);
        }

        /// <inheritdoc/>
        public override IEnumerable<IFeatureInfo> GetSupportedFeatures()
        {
            yield return new GenericFeatureInfo("MLST", FeatureStatus);
        }

        /// <inheritdoc/>
        public override IEnumerable<FtpCommandHandlerExtension> GetExtensions()
        {
            yield return new GenericFtpCommandHandlerExtension(Connection, "OPTS", "MLST", FeatureHandler);
        }

        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var argument = command.Argument;

            var path = Data.Path.Clone();
            IUnixFileSystemEntry targetEntry;

            if (string.IsNullOrEmpty(argument))
            {
                targetEntry = path.Count == 0 ? Data.FileSystem.Root : path.Peek();
            }
            else
            {
                var foundEntry = await Data.FileSystem.SearchEntryAsync(path, argument, cancellationToken);
                if (foundEntry?.Entry == null)
                    return new FtpResponse(550, "File system entry not found.");
                targetEntry = foundEntry.Entry;
            }

            var dirEntry = targetEntry as IUnixDirectoryEntry;
            var isDirEntry = dirEntry != null;

            var listDir = string.Equals(command.Name, "MLSD", StringComparison.OrdinalIgnoreCase);
            if (listDir && !isDirEntry)
                return new FtpResponse(501, "Not a directory.");

            await Connection.WriteAsync(new FtpResponse(150, "Opening data connection."), cancellationToken);
            ITcpSocketClient responseSocket;
            try
            {
                responseSocket = await Connection.CreateResponseSocket();
            }
            catch (Exception)
            {
                return new FtpResponse(425, "Can't open data connection.");
            }
            try
            {
                var formatter = new FactsListFormatter(Data.User, Data.FileSystem, path);

                var encoding = Data.NlstEncoding ?? Connection.Encoding;
                using (var stream = await Connection.CreateEncryptedStream(responseSocket.WriteStream))
                {
                    using (var writer = new StreamWriter(stream, encoding, 4096, true)
                    {
                        NewLine = "\r\n",
                    })
                    {
                        if (listDir)
                        {
                            foreach (var line in formatter.GetPrefix(dirEntry))
                            {
                                Connection.Log?.Debug(line);
                                await writer.WriteLineAsync(line);
                            }

                            foreach (var entry in await Data.FileSystem.GetEntriesAsync(dirEntry, cancellationToken))
                            {
                                var line = formatter.Format(entry);
                                Connection.Log?.Debug(line);
                                await writer.WriteLineAsync(line);
                            }

                            foreach (var line in formatter.GetSuffix(dirEntry))
                            {
                                Connection.Log?.Debug(line);
                                await writer.WriteLineAsync(line);
                            }
                        }
                        else
                        {
                            var line = formatter.Format(targetEntry);
                            Connection.Log?.Debug(line);
                            await writer.WriteLineAsync(line);
                        }
                        await writer.FlushAsync();
                    }
                    await stream.FlushAsync(cancellationToken);
                }
            }
            finally
            {
                responseSocket.Dispose();
            }

            // Use 250 when the connection stays open.
            return new FtpResponse(226, "Closing data connection.");
        }

        private static string FeatureStatus(FtpConnection connection)
        {
            var result = new StringBuilder();
            result.Append("MLST ");
            foreach (var fact in _knownFacts)
            {
                result.AppendFormat("{0}{1};", fact, connection.Data.ActiveMlstFacts.Contains(fact) ? "*" : string.Empty);
            }
            return result.ToString();
        }

        private Task<FtpResponse> FeatureHandler(FtpCommand command, CancellationToken cancellationToken)
        {
            var facts = command.Argument.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            Connection.Data.ActiveMlstFacts.Clear();
            foreach (var fact in facts)
            {
                if (!_knownFacts.Contains(fact))
                    return Task.FromResult(new FtpResponse(501, "Syntax error in parameters or arguments."));
                Connection.Data.ActiveMlstFacts.Add(fact);
            }
            return Task.FromResult(new FtpResponse(200, "Command okay."));
        }
    }
}
