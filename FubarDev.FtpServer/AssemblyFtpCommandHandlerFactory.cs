//-----------------------------------------------------------------------
// <copyright file="AssemblyFtpCommandHandlerFactory.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using FubarDev.FtpServer.CommandExtensions;
using FubarDev.FtpServer.CommandHandlers;

using JetBrains.Annotations;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Searches assemblies for <see cref="FtpCommandHandler"/> and <see cref="FtpCommandHandlerExtension"/> implementations
    /// </summary>
    public class AssemblyFtpCommandHandlerFactory : IFtpCommandHandlerFactory
    {
        private readonly IEnumerable<Assembly> _assemblies;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyFtpCommandHandlerFactory"/> class.
        /// </summary>
        /// <param name="assembly">The assembly to search for <see cref="FtpCommandHandler"/> and <see cref="FtpCommandHandlerExtension"/> implementations</param>
        /// <param name="assemblies">The other assembly to search for the <see cref="FtpCommandHandler"/> and <see cref="FtpCommandHandlerExtension"/> implementations</param>
        public AssemblyFtpCommandHandlerFactory([NotNull] Assembly assembly, [NotNull, ItemNotNull] params Assembly[] assemblies)
        {
            var asms = new List<Assembly>() { assembly };
            asms.AddRange(assemblies);
            _assemblies = asms;
        }

        /// <inheritdoc/>
        public virtual IEnumerable<FtpCommandHandler> CreateCommandHandlers(FtpConnection connection)
        {
            return
                from asm in _assemblies
                from type in asm.DefinedTypes
                where !type.IsAbstract && type.IsSubclassOf(typeof(FtpCommandHandler))
                select (FtpCommandHandler)Activator.CreateInstance(type.AsType(), connection);
        }

        /// <inheritdoc/>
        public virtual IEnumerable<FtpCommandHandlerExtension> CreateCommandHandlerExtensions(FtpConnection connection)
        {
            return
                from asm in _assemblies
                from type in asm.DefinedTypes
                where
                    !type.IsAbstract
                    && type.IsSubclassOf(typeof(FtpCommandHandlerExtension))
                    && type.AsType() != typeof(GenericFtpCommandHandlerExtension)
                select (FtpCommandHandlerExtension)Activator.CreateInstance(type.AsType(), connection);
        }
    }
}
