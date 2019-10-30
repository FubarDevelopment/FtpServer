// <copyright file="RepeatAttribute.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Linq;

namespace FubarDev.FtpServer.Tests
{
    public sealed class RepeatAttribute : Xunit.Sdk.DataAttribute
    {
        private readonly int _count;

        public RepeatAttribute(int count)
        {
            if (count < 1)
            {
                throw new System.ArgumentOutOfRangeException(
                    paramName: nameof(count),
                    message: "Repeat count must be greater than 0.");
            }

            _count = count;
        }

        public override System.Collections.Generic.IEnumerable<object[]> GetData(System.Reflection.MethodInfo testMethod)
        {
            return Enumerable.Range(start: 1, count: this._count)
               .Select(x => new object[] { x });
        }
    }
}
