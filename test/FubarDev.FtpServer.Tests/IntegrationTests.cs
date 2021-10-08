// <copyright file="IntegrationTests.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using FluentFTP;

using Xunit;
using Xunit.Abstractions;

namespace FubarDev.FtpServer.Tests
{
    public class IntegrationTests : FtpServerTestsBase
    {
        private IFtpClient? _client;

        public IntegrationTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        public IFtpClient Client => _client ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            _client = new FtpClient("127.0.0.1", Server.Port, "anonymous", "test@test.net");
            await _client.ConnectAsync();
        }

        /// <inheritdoc />
        public override async Task DisposeAsync()
        {
            if (_client != null)
            {
                await _client.DisconnectAsync();
            }

            await base.DisposeAsync();
        }

        /// <summary>
        /// Upload a test file.
        /// </summary>
        /// <param name="fileName">The name of the file to write.</param>
        /// <returns>The task.</returns>
        [Theory]
        [InlineData("设备管理-摄像机管理-w.txt")]
        public async Task TestUtf8FileNamesForUploadAsync(string fileName)
        {
            await Client.UploadAsync(
                Encoding.UTF8.GetBytes("Hello, this is a test!"),
                fileName);

            var fileNames = await Client.GetNameListingAsync();
            Assert.NotNull(fileNames);
            Assert.Collection(
                fileNames,
                item =>
                {
                    Debug.WriteLine(item.Length);
                    Debug.WriteLine(item);
                    Debug.WriteLine(char.ConvertToUtf32(item, 0));
                    Assert.Equal(".", item);
                },
                item => Assert.Equal(fileName, item));
        }

        /// <summary>
        /// Upload a test file.
        /// </summary>
        /// <returns>The task.</returns>
        [Fact]
        public async Task TestUploadAsync()
        {
            await Client.UploadAsync(
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
            await Client.UploadAsync(
                Encoding.UTF8.GetBytes("Hello, this is a test!"),
                "test.txt");
            var temp = new MemoryStream();
            await Client.DownloadAsync(
                temp,
                "test.txt");
            var readData = Encoding.UTF8.GetString(temp.ToArray());
            Assert.Equal("Hello, this is a test!", readData);
        }
    }
}
