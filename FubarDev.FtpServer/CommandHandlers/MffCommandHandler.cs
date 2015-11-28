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

using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.ListFormatters.Facts;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <code>MFF</code> command.
    /// </summary>
    public class MffCommandHandler : FtpCommandHandler
    {
        private static readonly Dictionary<string, CreateFactDelegate> _knownFacts = new Dictionary<string, CreateFactDelegate>(StringComparer.OrdinalIgnoreCase)
        {
            ["modify"] = value =>
            {
                DateTimeOffset dto;
                if (!value.TryParseTimestamp("UTC", out dto))
                    return null;
                return new ModifyFact(dto);
            },
            ["create"] = value =>
            {
                DateTimeOffset dto;
                if (!value.TryParseTimestamp("UTC", out dto))
                    return null;
                return new CreateFact(dto);
            },
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="MffCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection to create this command handler for</param>
        public MffCommandHandler(FtpConnection connection)
            : base(connection, "MFF")
        {
        }

        private delegate IFact CreateFactDelegate(string value);

        /// <inheritdoc/>
        public override IEnumerable<IFeatureInfo> GetSupportedFeatures()
        {
            yield return new GenericFeatureInfo("MFF", FeatureStatus);
        }

        /// <inheritdoc/>
        public override async Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var parts = command.Argument.Split(new[] { ' ' }, 2);
            if (parts.Length != 2)
                return new FtpResponse(551, "Facts or file name missing.");

            var factInfos = new Dictionary<string, string>();
            foreach (var factEntry in parts[0].Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var keyValue = factEntry.Split(new[] { '=' }, 2);
                if (keyValue.Length != 2)
                    return new FtpResponse(551, $"Invalid format for fact {factEntry}.");
                factInfos.Add(keyValue[0], keyValue[1]);
            }

            var path = parts[1];
            var currentPath = Data.Path.Clone();
            var fileInfo = await Data.FileSystem.SearchFileAsync(currentPath, path, cancellationToken);
            if (fileInfo?.Entry == null)
                return new FtpResponse(550, "File not found.");

            var facts = new List<IFact>();
            foreach (var factInfo in factInfos)
            {
                CreateFactDelegate createFact;
                if (!_knownFacts.TryGetValue(factInfo.Key, out createFact))
                    return new FtpResponse(551, $"Unsupported fact {factInfo.Key}.");
                var fact = createFact(factInfo.Value);
                facts.Add(fact);
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
                        return new FtpResponse(551, $"Unsupported fact {fact.Name}.");
                }
            }

            if (creationTime != null || modificationTime != null)
                await Data.FileSystem.SetMacTimeAsync(fileInfo.Entry, modificationTime, null, creationTime, cancellationToken);

            var fullName = currentPath.GetFullPath() + fileInfo.FileName;
            var responseText = new StringBuilder();
            foreach (var fact in facts)
                responseText.AppendFormat("{0}={1};", fact.Name, fact.Value);
            responseText.AppendFormat(" {0}", fullName);

            return new FtpResponse(213, responseText.ToString());
        }

        private static string FeatureStatus(FtpConnection connection)
        {
            var result = new StringBuilder();
            result.Append("MFF ");
            foreach (var fact in _knownFacts)
            {
                result.AppendFormat("{0};", fact.Key);
            }
            return result.ToString();
        }
    }
}
