// <copyright file="Issue82ProtocolViolation.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using Polly;

using Xunit;

namespace FubarDev.FtpServer.Tests.Issues
{
    public class Issue82ProtocolViolation : IClassFixture<FtpServerFixture>
    {
        private readonly IFtpServer _server;

        public Issue82ProtocolViolation(FtpServerFixture ftpServerFixture)
        {
            _server = ftpServerFixture.Server;
        }

        [Fact]
        public async Task TestParallelRequests()
        {
            var step = 50;
            for (var i = 0; i != 5000; i += step)
            {
                await TestParallel(step, i).ConfigureAwait(false);
            }
        }

        private async Task TestParallel(int maxTasks, int offset)
        {
            var rng = new Random();
            var requestPolicy = Policy
               .Handle<WebException>(e => e.Status != WebExceptionStatus.ProtocolError)
               .WaitAndRetryAsync(5, count => TimeSpan.FromMilliseconds(rng.Next(1, 1001)));

            var tasks = new List<Task>();
            for (var i = 0; i != maxTasks; i++)
            {
                var requestIndex = i;
                var task = Task.Run(() => requestPolicy.ExecuteAsync(() => GetFilesAsync(offset + requestIndex)));
                tasks.Add(task);
            }

            var exceptions = new List<Exception>();
            for (var requestIndex = 0; requestIndex != tasks.Count; ++requestIndex)
            {
                var task = tasks[requestIndex];
                try
                {
                    await task.ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    exceptions.Add(new Exception($"Failed request {offset + requestIndex}: {ex.Message}", ex));
                }
            }

            if (exceptions.Count != 0)
            {
                throw new AggregateException($"Failed tasks: {exceptions.Count}", exceptions);
            }
        }

        private async Task<IList<string>> GetFilesAsync(int index)
        {
            var requestUri = $"ftp://127.0.0.1:{_server.Port}";
            var request = (FtpWebRequest)WebRequest.Create(requestUri);
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = new NetworkCredential("anonymous", $"foo_{index:D5}@bar.com");
            request.KeepAlive = false;

            var files = new List<string>();
            using (var response = await request.GetResponseAsync().ConfigureAwait(false))
            {
                using var responseStream = response.GetResponseStream();
                using var reader = new StreamReader(responseStream);
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    files.Add(Path.GetFileName(line));
                }

                reader.Close();
                responseStream.Close();
            }

            return files;
        }
    }
}
