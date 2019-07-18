// <copyright file="SingleRootWithoutHomeAccountDirectoryQuery.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.FtpServer.FileSystem;

using Microsoft.Extensions.Options;

namespace FubarDev.FtpServer.AccountManagement.Directories.SingleRootWithoutHome
{
    /// <summary>
    /// All users share the same root and none has a home directory.
    /// </summary>
    public class SingleRootWithoutHomeAccountDirectoryQuery : IAccountDirectoryQuery
    {
        private readonly GenericAccountDirectories _accountDirectories;

        /// <summary>
        /// Initializes a new instance of the <see cref="SingleRootWithoutHomeAccountDirectoryQuery"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public SingleRootWithoutHomeAccountDirectoryQuery(
            IOptions<SingleRootWithoutHomeAccountDirectoryQueryOptions> options)
        {
            var opts = options.Value;
            _accountDirectories = new GenericAccountDirectories(opts.RootPath);
        }

        /// <inheritdoc />
        public IAccountDirectories GetDirectories(IAccountInformation accountInformation)
        {
            return _accountDirectories;
        }
    }
}
