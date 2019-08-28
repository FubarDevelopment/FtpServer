// <copyright file="IntegrationTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.IO;
using System.Text;
using System.Threading.Tasks;

using FluentFTP;

using Xunit;

namespace FubarDev.FtpServer.Tests
{
    public class IntegrationTests : IClassFixture<FtpServerFixture>, IAsyncLifetime
    {
        private readonly IFtpServer _server;
        private readonly IFtpClient _client;

        public IntegrationTests(FtpServerFixture ftpServerFixture)
        {
            _server = ftpServerFixture.Server;
            _client = new FtpClient("127.0.0.1", _server.Port, "anonymous", "test@test.net");
        }

        /// <inheritdoc />
        public Task InitializeAsync()
        {
            return _client.ConnectAsync();
        }

        /// <inheritdoc />
        public Task DisposeAsync()
        {
            return _client.DisconnectAsync();
        }

        /// <summary>
        /// Upload a test file.
        /// </summary>
        /// <returns>The task.</returns>
        [Fact]
        public async Task TestUploadAsync()
        {
            await _client.UploadAsync(
                Encoding.UTF8.GetBytes("Hello, this is a test!"),
                "test.txt");
        }

        /// <summary>
        /// Upload and download test file.
        /// </summary>
        /// <returns>The task.</returns>
        [Fact]
        public async Task TestUploadAndDownloadAsync()
        {
            await _client.UploadAsync(
                Encoding.UTF8.GetBytes("Hello, this is a test!"),
                "test.txt");
            var temp = new MemoryStream();
            await _client.DownloadAsync(
                temp,
                "test.txt");
            var readData = Encoding.UTF8.GetString(temp.ToArray());
            Assert.Equal("Hello, this is a test!", readData);
        }
    }
}
