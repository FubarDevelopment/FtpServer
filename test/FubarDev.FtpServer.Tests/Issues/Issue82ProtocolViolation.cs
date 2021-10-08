// <copyright file="Issue82ProtocolViolation.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

using Xunit;
using Xunit.Abstractions;

namespace FubarDev.FtpServer.Tests.Issues
{
    public class Issue82ProtocolViolation : FtpServerTestsBase
    {
        public Issue82ProtocolViolation(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task TestParallelRequests()
        {
            const int maxTasks = 100;
            var tasks = new List<Task>();
            for (var i = 0; i != maxTasks; i++)
            {
                var task = Task.Run(GetFilesAsync);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestSerialRequests()
        {
            const int maxTasks = 20;
            for (var i = 0; i != maxTasks; i++)
            {
                await GetFilesAsync();
            }
        }

        private async Task<IList<string>> GetFilesAsync()
        {
            var requestUri = $"ftp://127.0.0.1:{Server.Port}";
            var request = (FtpWebRequest)WebRequest.Create(requestUri);
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = new NetworkCredential("anonymous", "foo@bar.com");
            request.KeepAlive = false;

            var files = new List<string>();
            using var response = await request.GetResponseAsync().ConfigureAwait(false);
            await using var responseStream = response.GetResponseStream() ?? throw new InvalidOperationException();
            using var reader = new StreamReader(responseStream);
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                files.Add(Path.GetFileName(line));
            }

            reader.Close();
            responseStream.Close();

            return files;
        }
    }
}
