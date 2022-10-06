// <copyright file="SemaphoreSlimExt.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;

namespace Abc.FubarDev.FtpServer.Networking
{
    internal class SemaphoreSlimExt : SemaphoreSlim
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SemaphoreSlimExt" /> class.
        /// </summary>
        /// <param name="initialCount">The initial number of requests for the semaphore that can be granted concurrently.</param>
        public SemaphoreSlimExt(int initialCount)
            : base(initialCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SemaphoreSlimExt"/> class.
        /// </summary>
        /// <param name="initialCount">The initial number of requests for the semaphore that can be granted concurrently.</param>
        /// <param name="maxCount">The maximum number of requests for the semaphore that can be granted concurrently.</param>
        public SemaphoreSlimExt(int initialCount, int maxCount)
            : base(initialCount, maxCount)
        {
        }

        public bool IsDisposed { get; internal set; }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            IsDisposed = true;
        }
    }
}
