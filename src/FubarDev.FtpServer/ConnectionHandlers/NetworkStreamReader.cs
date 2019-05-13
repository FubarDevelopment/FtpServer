// <copyright file="NetworkStreamReader.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.ConnectionHandlers
{
    public class NetworkStreamReader : ICommunicationService
    {
        [NotNull]
        private readonly Stream _stream;

        [NotNull]
        private readonly PipeWriter _pipeWriter;

        [NotNull]
        private readonly CancellationTokenSource _jobStopped = new CancellationTokenSource();

        [NotNull]
        private readonly CancellationTokenSource _jobPaused = new CancellationTokenSource();

        private readonly CancellationToken _connectionClosed;

        [NotNull]
        private Task _task = Task.CompletedTask;

        public NetworkStreamReader(
            [NotNull] Stream stream,
            [NotNull] PipeWriter pipeWriter,
            CancellationToken connectionClosed)
        {
            _stream = stream;
            _pipeWriter = pipeWriter;
            _connectionClosed = connectionClosed;
        }

        public ConnectionStatus Status { get; private set; } = ConnectionStatus.ReadyToRun;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (Status != ConnectionStatus.ReadyToRun)
            {
                throw new InvalidOperationException($"Status must be {ConnectionStatus.ReadyToRun}, but was {Status}.");
            }

            _task = FillPipelineAsync(
                _stream,
                _pipeWriter,
                _connectionClosed,
                _jobStopped.Token,
                _jobPaused.Token,
                new Progress<ConnectionStatus>(status => Status = status));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (Status != ConnectionStatus.Running)
            {
                throw new InvalidOperationException($"Status must be {ConnectionStatus.ReadyToRun}, but was {Status}.");
            }

            _jobStopped.Cancel();

            return _task;
        }

        public Task PauseAsync(CancellationToken cancellationToken)
        {
            if (Status != ConnectionStatus.Running)
            {
                throw new InvalidOperationException($"Status must be {ConnectionStatus.ReadyToRun}, but was {Status}.");
            }

            _jobPaused.Cancel();

            return _task;
        }

        public Task ContinueAsync(CancellationToken cancellationToken)
        {
            if (Status != ConnectionStatus.Paused)
            {
                throw new InvalidOperationException($"Status must be {ConnectionStatus.ReadyToRun}, but was {Status}.");
            }

            _task = FillPipelineAsync(
                _stream,
                _pipeWriter,
                _connectionClosed,
                _jobStopped.Token,
                _jobPaused.Token,
                new Progress<ConnectionStatus>(status => Status = status));

            return Task.CompletedTask;
        }

        [NotNull]
        private static async Task FillPipelineAsync(
            [NotNull] Stream stream,
            [NotNull] PipeWriter writer,
            CancellationToken connectionClosed,
            CancellationToken jobStopped,
            CancellationToken jobPaused,
            IProgress<ConnectionStatus> statusProgress)
        {
            var globalCts = CancellationTokenSource.CreateLinkedTokenSource(connectionClosed, jobStopped, jobPaused);
            var buffer = new byte[1024];

            statusProgress.Report(ConnectionStatus.Running);

            Exception exception = null;
            while (true)
            {
                // Allocate at least 512 bytes from the PipeWriter
                var memory = writer.GetMemory(buffer.Length);
                try
                {
                    var bytesRead = await stream
                       .ReadAsync(buffer, 0, buffer.Length, globalCts.Token)
                       .ConfigureAwait(false);
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    buffer.AsSpan(0, bytesRead).CopyTo(memory.Span);

                    // Tell the PipeWriter how much was read from the Socket
                    writer.Advance(bytesRead);

                    // Make the data available to the PipeReader.
                    // Don't use the cancellation token source from above. Otherwise
                    // data might be lost.
                    var result = await writer.FlushAsync(CancellationToken.None);
                    if (result.IsCompleted)
                    {
                        break;
                    }
                }
                catch (Exception ex) when (IsOperationCancelledException(ex))
                {
                    // The job was aborted by one of the three cancellation tokens.
                    break;
                }
                catch (Exception ex)
                {
                    // Log?.LogError(ex, ex.Message);
                    exception = ex;
                    break;
                }
            }

            // Don't call Complete() when the job was just paused.
            if (jobPaused.IsCancellationRequested)
            {
                statusProgress.Report(ConnectionStatus.Paused);
                return;
            }

            // Tell the PipeReader that there's no more data coming
            writer.Complete(exception);

            statusProgress.Report(ConnectionStatus.Stopped);
        }

        private static bool IsOperationCancelledException(Exception ex)
        {
            switch (ex)
            {
                case OperationCanceledException _:
                    return true;
                case AggregateException aggEx:
                    return aggEx.InnerException is OperationCanceledException;
            }

            return false;
        }
    }
}
