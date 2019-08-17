// <copyright file="FtpResponseList{TStatus}.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Base class for FTP response lists.
    /// </summary>
    /// <typeparam name="TStatus">The type of the status used to get the lines.</typeparam>
    public abstract class FtpResponseList<TStatus> : IFtpResponse
        where TStatus : class
    {
        private readonly List<string> _result = new List<string>();

        private FtpResponseListStatus _buildStatus = FtpResponseListStatus.StartLine;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpResponseList{TStatus}"/> class.
        /// </summary>
        /// <param name="code">The status code.</param>
        /// <param name="startMessage">The message in the start line.</param>
        /// <param name="endMessage">The message in the end line.</param>
        protected FtpResponseList(
            int code,
            string startMessage,
            string endMessage)
        {
            StartMessage = startMessage;
            EndMessage = endMessage;
            Code = code;
        }

        /// <inheritdoc />
        public int Code { get; }

        /// <summary>
        /// Gets the async action to execute after sending the response to the client.
        /// </summary>
        [Obsolete("Use a custom server command.")]
        public FtpResponseAfterWriteAsyncDelegate? AfterWriteAction => null;

        /// <summary>
        /// Gets the message for the first line.
        /// </summary>
        public string StartMessage { get; }

        /// <summary>
        /// Gets the message for the last line.
        /// </summary>
        public string EndMessage { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            var lines = new[]
            {
                $"{Code}-{StartMessage.TrimEnd()}",
                " ... stripped ... async data",
                $"{Code} {EndMessage.TrimEnd()}",
            };

            return string.Join(Environment.NewLine, lines);
        }

        /// <inheritdoc />
        public async Task<FtpResponseLine> GetNextLineAsync(object? token, CancellationToken cancellationToken)
        {
            StatusStore? statusStore;

            if (token is null)
            {
                if (_buildStatus == FtpResponseListStatus.Finished)
                {
                    statusStore = new StatusStore
                    {
                        Enumerator = _result.GetEnumerator(),
                    };
                }
                else
                {
                    statusStore = new StatusStore()
                    {
                        Status = await CreateInitialStatusAsync(cancellationToken).ConfigureAwait(false),
                    };

                    _result.Clear();
                    _buildStatus = FtpResponseListStatus.StartLine;
                }
            }
            else
            {
                statusStore = (StatusStore)token;
            }

            string? resultLine;
            if (statusStore.Enumerator is null)
            {
                switch (_buildStatus)
                {
                    case FtpResponseListStatus.StartLine:
                        resultLine = $"{Code}-{StartMessage.TrimEnd()}";
                        _buildStatus = FtpResponseListStatus.Between;
                        break;

                    case FtpResponseListStatus.Between:
                        Debug.Assert(statusStore.Status != null, "statusStore.Status != null");
                        resultLine = await GetNextLineAsync(statusStore.Status!, cancellationToken)
                           .ConfigureAwait(false);
                        if (resultLine is null)
                        {
                            resultLine = $"{Code} {EndMessage.TrimEnd()}";
                            _buildStatus = FtpResponseListStatus.Finished;
                        }
                        else
                        {
                            resultLine = " " + resultLine;
                        }

                        break;

                    case FtpResponseListStatus.Finished:
                        resultLine = null;
                        statusStore = null;
                        break;

                    default:
                        throw new InvalidOperationException();
                }

                if (!(resultLine is null))
                {
                    _result.Add(resultLine);
                }
            }
            else
            {
                if (statusStore.Enumerator.MoveNext())
                {
                    resultLine = " " + statusStore.Enumerator.Current;
                }
                else
                {
                    statusStore.Enumerator.Dispose();
                    statusStore.IsFinished = true;
                    statusStore = null;
                    resultLine = null;
                }
            }

            return new FtpResponseLine(
                resultLine,
                statusStore);
        }

        /// <summary>
        /// Creates the initial status.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task containing the initial status.</returns>
        protected abstract Task<TStatus> CreateInitialStatusAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets the next line according to the given <paramref name="status"/>.
        /// </summary>
        /// <param name="status">The status used to get the next line.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The task containing the next line or <see langword="null"/> if there are no more lines.</returns>
        protected abstract Task<string?> GetNextLineAsync(TStatus status, CancellationToken cancellationToken);

        [DebuggerDisplay("Status store: Status = {Status}, IsFinished = {IsFinished}")]
        private class StatusStore
        {
            public TStatus? Status { get; set; }

            public IEnumerator<string>? Enumerator { get; set; }

            public bool IsFinished { private get; set; }
        }
    }
}
