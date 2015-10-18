//-----------------------------------------------------------------------
// <copyright file="ListSegment{T}.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FubarDev.FtpServer.FileSystem
{
    internal class ListSegment<T> : IReadOnlyList<T>
    {
        private readonly int _offset;

        public ListSegment(IReadOnlyList<T> list, int offset, int count)
        {
            List = list;
            _offset = offset;
            Count = count;
        }

        public IReadOnlyList<T> List { get; }

        public int Count { get; }

        public T this[int index] => List[_offset + index];

        public IEnumerator<T> GetEnumerator()
        {
            return List.Skip(_offset).Take(Count).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
