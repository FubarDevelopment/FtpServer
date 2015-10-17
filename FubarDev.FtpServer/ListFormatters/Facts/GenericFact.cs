// <copyright file="GenericFact.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer.ListFormatters.Facts
{
    public class GenericFact : IFact
    {
        public GenericFact(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name
        { get; }

        public string Value
        { get; }
    }
}
