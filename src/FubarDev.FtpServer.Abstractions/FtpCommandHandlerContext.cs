// <copyright file="FtpCommandHandlerContext.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer
{
    /// <summary>
    /// The context in which an FTP command gets executed.
    /// </summary>
    public class FtpCommandHandlerContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpCommandHandlerContext"/> class.
        /// </summary>
        /// <param name="ftpContext">The FTP context.</param>
        public FtpCommandHandlerContext(FtpContext ftpContext)
        {
            FtpContext = ftpContext;
        }

        /// <summary>
        /// Gets the FTP context.
        /// </summary>
        public FtpContext FtpContext { get; }
    }
}
