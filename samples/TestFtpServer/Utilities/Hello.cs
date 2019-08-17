// <copyright file="Hello.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using FubarDev.FtpServer;

namespace TestFtpServer.Utilities
{
    /// <summary>
    /// Creates a greeting response for all given names.
    /// </summary>
    public class Hello
    {
        private readonly ISet<string> _greetedPersons = new HashSet<string>();

        /// <summary>
        /// Creates greeting response for the given <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name to create the response for.</param>
        /// <returns>The created response.</returns>
        public IFtpResponse CreateResponse(string? name)
        {
            IFtpResponse response;
            if (string.IsNullOrEmpty(name))
            {
                if (_greetedPersons.Count == 0)
                {
                    response = new FtpResponse(200,"Hello, World!");
                }
                else
                {
                    response = new FtpResponseList(
                        211,
                        "Our \"Hello\" goes to:",
                        "END",
                        _greetedPersons);
                }
            }
            else
            {
                if (!_greetedPersons.Add(name!))
                {
                    response = new FtpResponse(200,$"Hello again, {name}!");
                }
                else
                {
                    response = new FtpResponse(200,$"Hello, {name}!");
                }
            }

            return response;
        }
    }
}
