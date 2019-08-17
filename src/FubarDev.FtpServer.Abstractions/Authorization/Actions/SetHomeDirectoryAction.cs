// <copyright file="SetHomeDirectoryAction.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.FileSystem;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FubarDev.FtpServer.Authorization.Actions
{
    /// <summary>
    /// Sets the home directory for the connection.
    /// </summary>
    public class SetHomeDirectoryAction : IAuthorizationAction
    {
        private readonly IFtpConnectionAccessor _ftpConnectionAccessor;

        private readonly IAccountDirectoryQuery _accountDirectoryQuery;

        private readonly ILogger<SetHomeDirectoryAction>? _logger;

        private readonly bool _createMissingDirectories;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetHomeDirectoryAction"/> class.
        /// </summary>
        /// <param name="options">The options for the <see cref="SetHomeDirectoryAction"/>.</param>
        /// <param name="ftpConnectionAccessor">The FTP connection accessor.</param>
        /// <param name="accountDirectoryQuery">Interface to query account directories.</param>
        /// <param name="logger">The logger.</param>
        public SetHomeDirectoryAction(
            IOptions<SetHomeDirectoryActionOptions> options,
            IFtpConnectionAccessor ftpConnectionAccessor,
            IAccountDirectoryQuery accountDirectoryQuery,
            ILogger<SetHomeDirectoryAction>? logger = null)
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
            var fsFeature = connection.Features.Get<IFileSystemFeature>();
            var fileSystem = fsFeature.FileSystem;
            var directories = _accountDirectoryQuery.GetDirectories(accountInformation);
            Stack<IUnixDirectoryEntry>? path = null;
            if (!string.IsNullOrEmpty(directories.HomePath))
            {
                _logger?.LogDebug("Requested home path is {homePath}", directories.HomePath);
                var selectResult = await fileSystem.SelectAsync(directories.HomePath, cancellationToken)
                   .ConfigureAwait(false);
                if (selectResult.ResultType == PathSelectionResultType.FoundDirectory)
                {
                    path = selectResult.GetPath();
                    _logger?.LogInformation("PWD set to {pwd}", path.ToDisplayString());
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

            fsFeature.Path = path ?? new Stack<IUnixDirectoryEntry>();
        }
    }
}
