// <copyright file="TlsStreamService.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.Authentication;

using JetBrains.Annotations;

namespace FubarDev.FtpServer.ConnectionHandlers
{
    public class TlsStreamService : ICommunicationService
    {
        [NotNull]
        private readonly IDuplexPipe _socketPipe;

        [NotNull]
        private readonly IDuplexPipe _connectionPipe;

        [NotNull]
        private readonly ISslStreamWrapperFactory _sslStreamWrapperFactory;

        [NotNull]
        private readonly X509Certificate2 _certificate;

        private readonly CancellationTokenSource _connectionClosedCts;

        [NotNull]
        private readonly CancellationTokenSource _jobStopped = new CancellationTokenSource();

        [NotNull]
        private CancellationTokenSource _jobPaused = new CancellationTokenSource();

        [NotNull]
        private Task _task = Task.CompletedTask;

        public TlsStreamService(
            [NotNull] IDuplexPipe socketPipe,
            [NotNull] IDuplexPipe connectionPipe,
            [NotNull] ISslStreamWrapperFactory sslStreamWrapperFactory,
            [NotNull] X509Certificate2 certificate,
            CancellationTokenSource connectionClosedCts)
        {
            _socketPipe = socketPipe;
            _connectionPipe = connectionPipe;
            _sslStreamWrapperFactory = sslStreamWrapperFactory;
            _certificate = certificate;
            _connectionClosedCts = connectionClosedCts;
        }

        /// <inheritdoc />
        public ConnectionStatus Status { get; private set; } = ConnectionStatus.ReadyToRun;

        /// <summary>
        /// Gets or sets a value indicating whether the SSL stream should be used.
        /// </summary>
        public bool EnableSslStream { get; set; }

        /// <inheritdoc />
        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (Status != ConnectionStatus.ReadyToRun)
            {
                throw new InvalidOperationException($"Status must be {ConnectionStatus.ReadyToRun}, but was {Status}.");
            }

            _task = RunAsync(new Progress<ConnectionStatus>(status => Status = status));

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (Status != ConnectionStatus.Running && Status != ConnectionStatus.Stopped && Status != ConnectionStatus.Paused)
            {
                throw new InvalidOperationException($"Status must be {ConnectionStatus.Running}, {ConnectionStatus.Stopped}, or {ConnectionStatus.Paused}, but was {Status}.");
            }

            _jobStopped.Cancel();

            return _task;
        }

        /// <inheritdoc />
        public async Task PauseAsync(CancellationToken cancellationToken)
        {
            if (Status != ConnectionStatus.Running)
            {
                throw new InvalidOperationException($"Status must be {ConnectionStatus.Running}, but was {Status}.");
            }

            _jobPaused.Cancel();

            await _task.ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task ContinueAsync(CancellationToken cancellationToken)
        {
            if (Status == ConnectionStatus.Stopped)
            {
                // Stay stopped!
                return Task.CompletedTask;
            }

            if (Status != ConnectionStatus.Paused)
            {
                throw new InvalidOperationException($"Status must be {ConnectionStatus.Paused}, but was {Status}.");
            }

            _jobPaused = new CancellationTokenSource();

            _task = RunAsync(new Progress<ConnectionStatus>(status => Status = status));

            return Task.CompletedTask;
        }

        private async Task RunAsync([NotNull] IProgress<ConnectionStatus> statusProgress)
        {
            statusProgress.Report(ConnectionStatus.Running);

            try
            {
                using (var globalCts = CancellationTokenSource.CreateLinkedTokenSource(
                    _connectionClosedCts.Token,
                    _jobStopped.Token,
                    _jobPaused.Token))
                {
                    Task task;
                    if (EnableSslStream)
                    {
                        task = EncryptAsync(
                            _socketPipe,
                            _connectionPipe,
                            _sslStreamWrapperFactory,
                            _certificate,
                            globalCts.Token);
                    }
                    else
                    {
                        task = PassThroughAsync(
                            _socketPipe,
                            _connectionPipe,
                            globalCts.Token);
                    }

                    await Task.WhenAny(task, Task.Delay(-1, globalCts.Token))
                       .ConfigureAwait(false);
                }

                // Don't call Complete() when the job was just paused.
                if (_jobPaused.IsCancellationRequested)
                {
                    statusProgress.Report(ConnectionStatus.Paused);
                    return;
                }
            }
            catch
            {
                // TODO: Logging!
                _connectionClosedCts.Cancel();
            }

            _socketPipe.Input.Complete();
            _socketPipe.Output.Complete();
            _connectionPipe.Input.Complete();
            _connectionPipe.Output.Complete();

            statusProgress.Report(ConnectionStatus.Stopped);
        }

        private static async Task EncryptAsync(
            [NotNull] IDuplexPipe socketPipe,
            [NotNull] IDuplexPipe connectionPipe,
            [NotNull] ISslStreamWrapperFactory sslStreamWrapperFactory,
            [NotNull] X509Certificate2 certificate,
            CancellationToken cancellationToken)
        {
            var rawStream = new RawStream(socketPipe.Input, socketPipe.Output);
            var sslStream = await sslStreamWrapperFactory.WrapStreamAsync(rawStream, false, certificate, cancellationToken)
               .ConfigureAwait(false);
            try
            {
                var copyToStream = CopyPipelineToStreamAsync(sslStream, connectionPipe.Input, cancellationToken);
                var copyToPipeline = CopyStreamToPipelineAsync(sslStream, connectionPipe.Output, cancellationToken);

                await Task.WhenAny(copyToStream, copyToPipeline, Task.Delay(-1, cancellationToken))
                   .ConfigureAwait(false);
                socketPipe.Input.CancelPendingRead();

                await Task.WhenAll(copyToStream, copyToPipeline)
                   .ConfigureAwait(false);
            }
            finally
            {
                await sslStreamWrapperFactory.CloseStreamAsync(sslStream, cancellationToken)
                   .ConfigureAwait(false);
            }
        }

        private static async Task PassThroughAsync(
            [NotNull] IDuplexPipe socketPipe,
            [NotNull] IDuplexPipe connectionPipe,
            CancellationToken cancellationToken)
        {
            var connectionToSocket = PassThroughAsync(
                connectionPipe.Input,
                socketPipe.Output,
                cancellationToken);
            var socketToConnection = PassThroughAsync(
                socketPipe.Input,
                connectionPipe.Output,
                cancellationToken);

            await Task.WhenAny(connectionToSocket, socketToConnection, Task.Delay(-1, cancellationToken))
               .ConfigureAwait(false);
            socketPipe.Input.CancelPendingRead();
            connectionPipe.Input.CancelPendingRead();

            await Task.WhenAll(connectionToSocket, socketToConnection)
               .ConfigureAwait(false);
        }

        private static async Task PassThroughAsync(
            [NotNull] PipeReader reader,
            [NotNull] PipeWriter writer,
            CancellationToken cancellationToken)
        {
            while (true)
            {
                // Allocate at least 512 bytes from the PipeWriter
                try
                {
                    var readResult = await reader.ReadAsync(cancellationToken)
                       .ConfigureAwait(false);

                    var buffer = readResult.Buffer;
                    var position = buffer.Start;

                    while (buffer.TryGet(ref position, out var memory))
                    {
                        // Don't use the cancellation token source from above. Otherwise
                        // data might be lost.
                        await writer.WriteAsync(memory, CancellationToken.None)
                           .ConfigureAwait(false);
                    }

                    reader.AdvanceTo(buffer.End);

                    if (readResult.IsCanceled || readResult.IsCompleted)
                    {
                        break;
                    }
                }
                catch (Exception ex) when (ex.IsOperationCancelledException())
                {
                    // The job was aborted by one of the three cancellation tokens.
                    break;
                }
            }
        }

        [NotNull]
        private static async Task CopyStreamToPipelineAsync(
            [NotNull] Stream stream,
            [NotNull] PipeWriter writer,
            CancellationToken cancellationToken)
        {
            var buffer = new byte[1024];
            while (true)
            {
                // Allocate at least 512 bytes from the PipeWriter
                var memory = writer.GetMemory(buffer.Length);
                try
                {
                    var readTask = stream
                       .ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                    var resultTask = await Task.WhenAny(readTask, Task.Delay(-1, cancellationToken))
                       .ConfigureAwait(false);
                    if (resultTask != readTask)
                    {
                        break;
                    }

                    var bytesRead = readTask.Result;

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

                    if (result.IsCanceled)
                    {
                        break;
                    }
                }
                catch (Exception ex) when (ex.IsIOException())
                {
                    // Connection closed?
                    break;
                }
                catch (Exception ex) when (ex.IsOperationCancelledException())
                {
                    // The job was aborted by one of the three cancellation tokens.
                    break;
                }
            }
        }

        private static async Task CopyPipelineToStreamAsync(
            [NotNull] Stream stream,
            [NotNull] PipeReader reader,
            CancellationToken cancellationToken)
        {
            while (true)
            {
                // Allocate at least 512 bytes from the PipeWriter
                try
                {
                    var readResult = await reader.ReadAsync(cancellationToken)
                       .ConfigureAwait(false);

                    // Don't use the cancellation token source from above. Otherwise
                    // data might be lost.
                    await SendDataToStream(readResult.Buffer, stream, CancellationToken.None)
                       .ConfigureAwait(false);

                    reader.AdvanceTo(readResult.Buffer.End);

                    if (readResult.IsCanceled || readResult.IsCompleted)
                    {
                        break;
                    }
                }
                catch (Exception ex) when (ex.IsOperationCancelledException())
                {
                    // The job was aborted by one of the three cancellation tokens.
                    break;
                }
            }

            try
            {
                await FlushAsync(stream, reader, CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex.IsIOException())
            {
                // Ignored. Connection closed by client?
            }
            catch
            {
                // TODO: Warning about an error while flushing.
            }
        }

        [NotNull]
        private static async Task FlushAsync(
            [NotNull] Stream stream,
            [NotNull] PipeReader reader,
            CancellationToken cancellationToken)
        {
            while (reader.TryRead(out var readResult))
            {
                await SendDataToStream(readResult.Buffer, stream, cancellationToken)
                   .ConfigureAwait(false);
                reader.AdvanceTo(readResult.Buffer.End);
            }
        }

        [NotNull]
        private static async Task SendDataToStream(
            ReadOnlySequence<byte> buffer,
            [NotNull] Stream stream,
            CancellationToken cancellationToken)
        {
            var position = buffer.Start;

            while (buffer.TryGet(ref position, out var memory))
            {
                var streamBuffer = memory.ToArray();
                await stream.WriteAsync(streamBuffer, 0, streamBuffer.Length, cancellationToken)
                   .ConfigureAwait(false);
            }

            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
