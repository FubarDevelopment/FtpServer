//-----------------------------------------------------------------------
// <copyright file="MffCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Commands;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.ListFormatters.Facts;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <c>MFF</c> command.
    /// </summary>
    [FtpCommandHandler("MFF")]
    [FtpFeatureFunction(nameof(FeatureStatus))]
    public class MffCommandHandler : FtpCommandHandler
    {
        private static readonly Dictionary<string, CreateFactDelegate> _knownFacts = new Dictionary<string, CreateFactDelegate>(StringComparer.OrdinalIgnoreCase)
        {
            ["modify"] = value =>
            {
                if (!value.TryParseTimestamp("UTC", out var dto))
                {
                    return null;
                }

                return new ModifyFact(dto);
            },
            ["create"] = value =>
            {
                if (!value.TryParseTimestamp("UTC", out var dto))
                {
                    return null;
                }

                return new CreateFact(dto);
            },
        };

        private delegate IFact? CreateFactDelegate(string value);

        /// <summary>
        /// Gets the feature string for the <c>MFF</c> command.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns>The feature string.</returns>
        public static string FeatureStatus(IFtpConnection connection)
        {
            var result = new StringBuilder();
            result.Append("MFF ");
            foreach (var fact in _knownFacts)
            {
                result.AppendFormat("{0};", fact.Key);
            }
            return result.ToString();
        }

        /// <inheritdoc/>
        public override async Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var parts = command.Argument.Split(new[] { ' ' }, 2);
            if (parts.Length != 2)
            {
                return new FtpResponse(551, T("Facts or file name missing."));
            }

            var factInfos = new Dictionary<string, string>();
            foreach (var factEntry in parts[0].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var keyValue = factEntry.Split(new[] { '=' }, 2);
                if (keyValue.Length != 2)
                {
                    return new FtpResponse(551, T("Invalid format for fact {0}.", factEntry));
                }

                factInfos.Add(keyValue[0], keyValue[1]);
            }

            var fsFeature = Connection.Features.Get<IFileSystemFeature>();

            var path = parts[1];
            var currentPath = fsFeature.Path.Clone();
            var fileInfo = await fsFeature.FileSystem.SearchFileAsync(currentPath, path, cancellationToken).ConfigureAwait(false);
            if (fileInfo?.Entry == null)
            {
                return new FtpResponse(550, T("File not found."));
            }

            var facts = new List<IFact>();
            foreach (var factInfo in factInfos)
            {
                if (!_knownFacts.TryGetValue(factInfo.Key, out var createFact))
                {
                    return new FtpResponse(551, T("Unsupported fact {0}.", factInfo.Key));
                }

                var fact = createFact(factInfo.Value);
                if (fact != null)
                {
                    facts.Add(fact);
                }
            }

            DateTimeOffset? modificationTime = null, creationTime = null;
            foreach (var fact in facts)
            {
                switch (fact.Name.ToLowerInvariant())
                {
                    case "modify":
                        modificationTime = ((ModifyFact)fact).Timestamp;
                        break;
                    case "create":
                        creationTime = ((CreateFact)fact).Timestamp;
                        break;
                    default:
                        return new FtpResponse(551, T("Unsupported fact {0}.", fact.Name));
                }
            }

            if (creationTime != null || modificationTime != null)
            {
                await fsFeature.FileSystem.SetMacTimeAsync(fileInfo.Entry, modificationTime, null, creationTime, cancellationToken).ConfigureAwait(false);
            }

            var fullName = currentPath.GetFullPath() + fileInfo.FileName;
            var responseText = new StringBuilder();
            foreach (var fact in facts)
            {
                responseText.AppendFormat("{0}={1};", fact.Name, fact.Value);
            }

            responseText.AppendFormat(" {0}", fullName);

            return new FtpResponse(213, responseText.ToString());
        }
    }
}
