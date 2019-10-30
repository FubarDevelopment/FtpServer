// <copyright file="Issue82ProtocolViolation.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

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
            var tasks = new List<Task>();
            var maxTasks = 100;
            for (var i = 0; i != maxTasks; i++)
            {
                var taskIndex = i;
                var task = Task.Run(() => GetFilesAsync(taskIndex));
                tasks.Add(task);
            }

            var errors = new List<Exception>();
            foreach (var task in tasks)
            {
                try
                {
                    await task.ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }
            }

            if (errors.Count != 0)
            {
                throw new AggregateException(errors);
            }
        }

        private async Task<IList<string>> GetFilesAsync(int taskIndex)
        {
            var requestUri = $"ftp://127.0.0.1:{_server.Port}";
            var request = (FtpWebRequest)WebRequest.Create(requestUri);
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = new NetworkCredential("anonymous", $"foo{taskIndex}@bar.com");
            request.KeepAlive = false;

            var files = new List<string>();
            try
            {
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
            }
            catch (Exception ex)
            {
                throw new Exception($"Test {taskIndex} failed with: {ex.Message}", ex);
            }

            return files;
        }
    }
}
