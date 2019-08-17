// <copyright file="AssemblyFtpCommandHandlerScanner.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FubarDev.FtpServer.Commands
{
    /// <summary>
    /// Scans the given assemblies for FTP command handlers.
    /// </summary>
    public class AssemblyFtpCommandHandlerScanner : IFtpCommandHandlerScanner
    {
        private readonly Assembly[] _assemblies;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyFtpCommandHandlerScanner"/> class.
        /// </summary>
        /// <param name="assemblies">The assemblies to scan for FTP command handlers.</param>
        public AssemblyFtpCommandHandlerScanner(params Assembly[] assemblies)
        {
            _assemblies = assemblies;
        }

        /// <inheritdoc />
        public IEnumerable<IFtpCommandHandlerInformation> Search()
        {
            return _assemblies.SelectMany(Search);
        }

        private static IEnumerable<IFtpCommandHandlerInformation> Search(Assembly assembly)
        {
            foreach (var typeInfo in assembly.DefinedTypes.Where(IsCommandHandlerClass))
            {
                var attributes = typeInfo.GetCustomAttributes<FtpCommandHandlerAttribute>().ToList();
                if (attributes.Count == 0)
                {
                    // Missing attribute?
                    continue;
                }

                foreach (var attribute in attributes)
                {
                    yield return typeInfo.AsType().GetInformation(attribute);
                }
            }
        }

        private static bool IsCommandHandlerClass(TypeInfo typeInfo)
        {
            if (!typeInfo.IsClass)
            {
                return false;
            }

            if (typeInfo.IsAbstract)
            {
                return false;
            }

            if (typeInfo.IsGenericTypeDefinition)
            {
                return false;
            }

            if (!typeof(IFtpCommandHandler).GetTypeInfo().IsAssignableFrom(typeInfo))
            {
                return false;
            }

            return true;
        }
    }
}
