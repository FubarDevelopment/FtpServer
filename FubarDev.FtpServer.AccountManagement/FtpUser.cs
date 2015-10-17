// <copyright file="FtpUser.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.AccountManagement
{
    /// <summary>
    /// A basic FTP user object
    /// </summary>
    public class FtpUser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpUser"/> class.
        /// </summary>
        /// <param name="name">The users name</param>
        public FtpUser(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the name of the user
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Returns <code>true</code> when the user is in the given group.
        /// </summary>
        /// <param name="groupName">The name of the group</param>
        /// <returns><code>true</code> when the user is in the queries <paramref name="group"/></returns>
        public virtual bool IsInGroup(string groupName)
        {
            return false;
        }
    }
}
