// <copyright file="FtpUser.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Text;

namespace TestFtpServer.Api
{
    /// <summary>
    /// FTP user information.
    /// </summary>
    public class FtpUser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpUser"/> class.
        /// </summary>
        /// <param name="name">The user name.</param>
        public FtpUser(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the user name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets the E-Mail address of the user.
        /// </summary>
        public string? Email { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            var result = new StringBuilder();
            result.Append(Name);
            if (!string.IsNullOrWhiteSpace(Email))
            {
                result.AppendFormat(" ({0})", Email);
            }

            return result.ToString();
        }
    }
}
