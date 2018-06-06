//-----------------------------------------------------------------------
// <copyright file="UserCommandHandler.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.AccountManagement;

namespace FubarDev.FtpServer.CommandHandlers
{
    /// <summary>
    /// Implements the <code>USER</code> command.
    /// </summary>
    public class UserCommandHandler : FtpCommandHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserCommandHandler"/> class.
        /// </summary>
        /// <param name="connection">The connection to create this command handler for.</param>
        public UserCommandHandler(IFtpConnection connection)
            : base(connection, "USER")
        {
        }

        /// <inheritdoc/>
        public override bool IsLoginRequired => false;

        /// <inheritdoc/>
        public override Task<FtpResponse> Process(FtpCommand command, CancellationToken cancellationToken)
        {
            var userName = command.Argument;
            Connection.Data.User = new UnauthenticatedUser(userName);
            return Task.FromResult(new FtpResponse(331, $"User {userName} logged in, needs password"));
        }

        private class UnauthenticatedUser : IFtpUser
        {
            public UnauthenticatedUser(string name)
            {
                Name = name;
            }

            public string Name { get; }

            public bool IsInGroup(string groupName) => false;
        }
    }
}
