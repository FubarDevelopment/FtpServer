// <copyright file="BackgroundUploadsInfo.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

using FubarDev.FtpServer.BackgroundTransfer;

namespace TestFtpServer.ServerInfo
{
    /// <summary>
    /// Information about the background transfer worker.
    /// </summary>
    public class BackgroundUploadsInfo : IExtendedModuleInfo, ISimpleModuleInfo
    {
        private readonly IBackgroundTransferWorker _backgroundTransferWorker;

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundUploadsInfo"/>.
        /// </summary>
        /// <param name="backgroundTransferWorker">The background transfer worker to get the information from.</param>
        public BackgroundUploadsInfo(
            IBackgroundTransferWorker backgroundTransferWorker)
        {
            _backgroundTransferWorker = backgroundTransferWorker;
        }

        /// <inheritdoc />
        public string Name { get; } = "background-uploads";

        /// <inheritdoc />
        public IEnumerable<(string label, string value)> GetInfo()
        {
            var states = _backgroundTransferWorker.GetStates();
            yield return ("Background uploads", $"{states.Count}");
        }

        /// <inheritdoc />
        public IEnumerable<string> GetExtendedInfo()
        {
            var states = _backgroundTransferWorker.GetStates();
            foreach (var info in states)
            {
                yield return $"File {info.FileName}";
                yield return $"\tStatus={info.Status}";
                if (info.Transferred != null)
                {
                    yield return $"\tTransferred={info.Transferred.Value}";
                }
            }
        }
    }
}
