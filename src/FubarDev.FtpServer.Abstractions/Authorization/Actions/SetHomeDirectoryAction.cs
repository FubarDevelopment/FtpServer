// <copyright file="SetHomeDirectoryAction.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.FileSystem;

using JetBrains.Annotations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FubarDev.FtpServer.Authorization.Actions
{
    public class SetHomeDirectoryAction : IAuthorizationAction
    {
        [NotNull]
        private readonly IFtpConnectionAccessor _ftpConnectionAccessor;

        [NotNull]
        private readonly IAccountDirectoryQuery _accountDirectoryQuery;

        [CanBeNull]
        private readonly ILogger<SetHomeDirectoryAction> _logger;

        private readonly bool _createMissingDirectories;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetHomeDirectoryAction"/> class.
        /// </summary>
        /// <param name="options">The options for the <see cref="SetHomeDirectoryAction"/>.</param>
        /// <param name="ftpConnectionAccessor">The FTP connection accessor.</param>
        /// <param name="accountDirectoryQuery">Interface to query account directories.</param>
        /// <param name="logger">The logger.</param>
        public SetHomeDirectoryAction(
            [NotNull] IOptions<SetHomeDirectoryActionOptions> options,
            [NotNull] IFtpConnectionAccessor ftpConnectionAccessor,
            [NotNull] IAccountDirectoryQuery accountDirectoryQuery,
            [CanBeNull] ILogger<SetHomeDirectoryAction> logger = null)
        {
            _ftpConnectionAccessor = ftpConnectionAccessor;
            _accountDirectoryQuery = accountDirectoryQuery;
            _logger = logger;
            _createMissingDirectories = options.Value.CreateMissingDirectories;
        }

        /// <inheritdoc />
        public int Level { get; } = 1700;

        /// <inheritdoc />
        public async Task AuthorizedAsync(IAccountInformation accountInformation, CancellationToken cancellationToken)
        {
            var connection = _ftpConnectionAccessor.FtpConnection;
            var fileSystem = connection.Data.FileSystem;
            var directories = _accountDirectoryQuery.GetDirectories(accountInformation);
            Stack<IUnixDirectoryEntry> path = null;
            if (!string.IsNullOrEmpty(directories.HomePath))
            {
                var selectResult = await fileSystem.SelectAsync(directories.HomePath, cancellationToken)
                   .ConfigureAwait(false);
                if (selectResult.ResultType == PathSelectionResultType.FoundDirectory)
                {
                    path = selectResult.GetPath();
                }
                else
                {
                    if (_createMissingDirectories)
                    {
                        path = selectResult.GetPath();
                        var currentDirectory = selectResult.Directory;
                        foreach (var missingName in selectResult.MissingNames)
                        {
                            var newDirectoryName = missingName.EndsWith("/")
                                ? missingName.Substring(0, missingName.Length - 1)
                                : missingName;
                            var newDirectory = await fileSystem.CreateDirectoryAsync(currentDirectory, newDirectoryName, cancellationToken)
                               .ConfigureAwait(false);
                            currentDirectory = newDirectory;
                            path.Push(newDirectory);
                        }
                    }
                    else
                    {
                        _logger?.LogWarning("The directory couldn't be set. Selector status was {status}", selectResult.ResultType);
                    }
                }
            }

            connection.Data.Path = path ?? new Stack<IUnixDirectoryEntry>();
        }
    }
}
