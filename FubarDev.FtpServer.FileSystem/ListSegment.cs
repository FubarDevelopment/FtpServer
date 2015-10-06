//-----------------------------------------------------------------------
// <copyright file="ListSegment.cs" company="Fubar Development Junker">
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

        /// <summary>
        /// Ruft die Anzahl der Elemente in der Auflistung ab.
        /// </summary>
        /// <returns>
        /// Die Anzahl der Elemente in der Auflistung.
        /// </returns>
        public int Count { get; }

        /// <summary>
        /// Ruft das Element am angegebenen Index in der schreibgeschützten Liste ab.
        /// </summary>
        /// <returns>
        /// Das Element am angegebenen Index in der schreibgeschützten Liste.
        /// </returns>
        /// <param name="index">Der nullbasierte Index des abzurufenden Elements.</param>
        public T this[int index] => List[_offset + index];

        /// <summary>
        /// Gibt einen Enumerator zurück, der die Auflistung durchläuft.
        /// </summary>
        /// <returns>
        /// Ein <see cref="T:System.Collections.Generic.IEnumerator`1"/>, der zum Durchlaufen der Auflistung verwendet werden kann.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return List.Skip(_offset).Take(Count).GetEnumerator();
        }

        /// <summary>
        /// Gibt einen Enumerator zurück, der eine Auflistung durchläuft.
        /// </summary>
        /// <returns>
        /// Ein <see cref="T:System.Collections.IEnumerator"/>-Objekt, das zum Durchlaufen der Auflistung verwendet werden kann.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
