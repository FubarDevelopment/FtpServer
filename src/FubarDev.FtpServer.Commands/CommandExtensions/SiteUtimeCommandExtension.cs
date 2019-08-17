// <copyright file="SiteUtimeCommandExtension.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.FileSystem;

namespace FubarDev.FtpServer.CommandExtensions
{
    /// <summary>
    /// The implementation of the <c>SITE UTIME</c> command.
    /// </summary>
    /// <remarks>
    /// This doesn't exist as RFC. Instead, it's only documented
    /// on the <a href="http://www.proftpd.org/docs/contrib/mod_site_misc.html">ProFTPd site</a>.
    /// This extension is hidden, according to https://ghisler.ch/board/viewtopic.php?t=24952.
    /// </remarks>
    [FtpCommandHandlerExtension("UTIME", "SITE")]
    public class SiteUtimeCommandExtension : FtpCommandHandlerExtension
    {
        /// <inheritdoc />
        public override void InitializeConnectionData()
        {
        }

        /// <inheritdoc/>
        public override async Task<IFtpResponse?> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(command.Argument))
            {
                return new FtpResponse(501, T("No file name."));
            }

            var parts = new List<string>();
            var remaining = command.Argument.ChompFromEnd(out var part);
            if (IsSupportedTimeZome(part))
            {
                // 5 part format
                // SITE <sp> UTIME <sp> filename <sp> datetime1 <sp> datetime2 <sp> datetime3 <sp> UTC
                parts.Add(part);
                for (var i = 0; i != 3; ++i)
                {
                    remaining = remaining.ChompFromEnd(out part);
                    parts.Add(remaining);
                }
                parts.Add(remaining);
                parts.Reverse();
                return await SetTimestamp5(parts, cancellationToken).ConfigureAwait(false);
            }

            parts.AddRange(command.Argument.Split(new[] { ' ' }, 2));
            while (parts.Count != 2)
            {
                parts.Add(string.Empty);
            }

            return await SetTimestamp2(parts, cancellationToken).ConfigureAwait(false);
        }

        private static bool IsSupportedTimeZome(string timezone)
        {
            return timezone == "UTC";
        }

        private async Task<FtpResponse> SetTimestamp5(IReadOnlyList<string> parts, CancellationToken cancellationToken)
        {
            if (!parts[1].TryParseTimestamp(parts[4], out var accessTime))
            {
                return new FtpResponse(501, T("Syntax error in parameters or arguments."));
            }

            if (!parts[2].TryParseTimestamp(parts[4], out var modificationTime))
            {
                return new FtpResponse(501, T("Syntax error in parameters or arguments."));
            }

            if (!parts[3].TryParseTimestamp(parts[4], out var creationTime))
            {
                return new FtpResponse(501, T("Syntax error in parameters or arguments."));
            }

            var path = parts[0];
            if (path.Length >= 2 && path.StartsWith("\"") && path.EndsWith("\""))
            {
                path = path.Substring(1, path.Length - 2);
            }

            if (string.IsNullOrEmpty(path))
            {
                return new FtpResponse(501, T("No file name."));
            }

            var fsFeature = Connection.Features.Get<IFileSystemFeature>();
            var currentPath = fsFeature.Path.Clone();
            var foundEntry = await fsFeature.FileSystem.SearchEntryAsync(currentPath, path, cancellationToken).ConfigureAwait(false);
            if (foundEntry?.Entry == null)
            {
                return new FtpResponse(550, T("File system entry not found."));
            }

            await fsFeature.FileSystem.SetMacTimeAsync(foundEntry.Entry, modificationTime, accessTime, creationTime, cancellationToken).ConfigureAwait(false);

            return new FtpResponse(220, T("Timestamps set."));
        }

        private async Task<FtpResponse> SetTimestamp2(IReadOnlyList<string> parts, CancellationToken cancellationToken)
        {
            if (!parts[0].TryParseTimestamp("UTC", out var modificationTime))
            {
                return new FtpResponse(501, T("Syntax error in parameters or arguments."));
            }

            var path = parts[1];
            if (path.Length >= 2 && path.StartsWith("\"") && path.EndsWith("\""))
            {
                path = path.Substring(1, path.Length - 2);
            }

            if (string.IsNullOrEmpty(path))
            {
                return new FtpResponse(501, T("No file name."));
            }

            var fsFeature = Connection.Features.Get<IFileSystemFeature>();
            var currentPath = fsFeature.Path.Clone();
            var foundEntry = await fsFeature.FileSystem.SearchEntryAsync(currentPath, path, cancellationToken).ConfigureAwait(false);
            if (foundEntry?.Entry == null)
            {
                return new FtpResponse(550, T("File system entry not found."));
            }

            await fsFeature.FileSystem.SetMacTimeAsync(foundEntry.Entry, modificationTime, null, null, cancellationToken).ConfigureAwait(false);

            return new FtpResponse(220, T("Modification time set."));
        }
    }
}
