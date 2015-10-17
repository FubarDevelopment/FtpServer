// <copyright file="IFact.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.FtpServer.ListFormatters.Facts
{
    public interface IFact
    {
        string Name
        { get; }

        string Value
        { get; }
    }
}
