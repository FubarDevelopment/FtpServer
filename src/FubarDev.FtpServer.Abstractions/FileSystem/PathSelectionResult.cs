// <copyright file="PathSelectionResult.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FubarDev.FtpServer.FileSystem
{
    /// <summary>
    /// The result of a <see cref="PathSelector.SelectAsync(IUnixFileSystem,string?,System.Threading.CancellationToken)"/> operation.
    /// </summary>
    public class PathSelectionResult
    {
        private readonly IUnixFileEntry? _document;
        private readonly Stack<IUnixDirectoryEntry> _foundPathSegments;
        private readonly IReadOnlyCollection<string> _missingPathSegments;

        internal PathSelectionResult(
            PathSelectionResultType resultType,
            IUnixFileEntry? document,
            Stack<IUnixDirectoryEntry> foundPathSegments,
            IReadOnlyCollection<string>? missingPathSegments)
        {
            ResultType = resultType;
            _document = document;
            _foundPathSegments = foundPathSegments;
            _missingPathSegments = missingPathSegments ?? Array.Empty<string>();
        }

        /// <summary>
        /// Gets the type of the result.
        /// </summary>
        public PathSelectionResultType ResultType { get; }

        /// <summary>
        /// Gets a value indicating whether there was a missing path part.
        /// </summary>
        public bool IsMissing =>
            ResultType == PathSelectionResultType.MissingDirectory ||
            ResultType == PathSelectionResultType.MissingFileOrDirectory;

        /// <summary>
        /// Gets the directory of the search result.
        /// </summary>
        /// <remarks>
        /// When <see cref="ResultType"/> is <see cref="PathSelectionResultType.FoundDirectory"/>, this is the found directory.
        /// When <see cref="ResultType"/> is <see cref="PathSelectionResultType.FoundFile"/>, this is the parent directory.
        /// Otherwise, this is the last found directory.
        /// </remarks>
        public IUnixDirectoryEntry Directory => _foundPathSegments.Peek();

        /// <summary>
        /// Gets the found document.
        /// </summary>
        /// <remarks>
        /// This property is only valid when <see cref="ResultType"/> is <see cref="PathSelectionResultType.FoundFile"/>.
        /// </remarks>
        public IUnixFileEntry? File
        {
            get
            {
                if (ResultType != PathSelectionResultType.FoundFile)
                {
                    throw new InvalidOperationException();
                }

                return _document;
            }
        }

        /// <summary>
        /// Gets the directory of missing child elements.
        /// </summary>
        /// <remarks>
        /// This is only valid, when <see cref="IsMissing"/> is <see langword="true"/>.
        /// </remarks>
        public IReadOnlyCollection<string> MissingNames
        {
            get
            {
                if (ResultType != PathSelectionResultType.MissingDirectory && ResultType != PathSelectionResultType.MissingFileOrDirectory)
                {
                    throw new InvalidOperationException();
                }

                return _missingPathSegments;
            }
        }

        /// <summary>
        /// Gets the full root-relative path of the element that was searched.
        /// </summary>
        public string FullPath
        {
            get
            {
                var result = new StringBuilder();

                foreach (var foundPathSegment in _foundPathSegments.Reverse())
                {
                    result.Append(foundPathSegment.GetPathSegment());
                }

                foreach (var missingPathSegment in _missingPathSegments)
                {
                    result.Append(missingPathSegment);
                }

                return result.ToString();
            }
        }

        /// <summary>
        /// Gets the found target entry.
        /// </summary>
        /// <remarks>
        /// This is only valid when <see cref="IsMissing"/> is <see langword="false"/>.
        /// </remarks>
        public IUnixFileSystemEntry TargetEntry
        {
            get
            {
                if (IsMissing)
                {
                    throw new InvalidOperationException();
                }

                if (ResultType == PathSelectionResultType.FoundFile)
                {
                    Debug.Assert(File != null, "File != null");
                    return File ?? throw new InvalidOperationException("File is null even though a file was found?");
                }

                return Directory;
            }
        }

        /// <summary>
        /// Creates a selection result for a found file.
        /// </summary>
        /// <param name="foundPathSegments">The found path segments.</param>
        /// <param name="document">The found file.</param>
        /// <returns>The created selection result.</returns>
        public static PathSelectionResult Create(
            Stack<IUnixDirectoryEntry> foundPathSegments,
            IUnixFileEntry document)
        {
            return new PathSelectionResult(
                PathSelectionResultType.FoundFile,
                document ?? throw new ArgumentNullException(nameof(document)),
                foundPathSegments ?? throw new ArgumentNullException(nameof(foundPathSegments)),
                null);
        }

        /// <summary>
        /// Creates a selection result for a found directory.
        /// </summary>
        /// <param name="foundPathSegments">The found path segments.</param>
        /// <returns>The created selection result.</returns>
        public static PathSelectionResult Create(
            Stack<IUnixDirectoryEntry> foundPathSegments)
        {
            return new PathSelectionResult(
                PathSelectionResultType.FoundDirectory,
                null,
                foundPathSegments ?? throw new ArgumentNullException(nameof(foundPathSegments)),
                null);
        }

        /// <summary>
        /// Creates a selection for a missing file or directory.
        /// </summary>
        /// <param name="foundPathSegments">The found path segments.</param>
        /// <param name="missingPathSegments">The missing path elements.</param>
        /// <returns>The created selection result.</returns>
        public static PathSelectionResult CreateMissingFileOrDirectory(
            Stack<IUnixDirectoryEntry> foundPathSegments,
            IReadOnlyCollection<string> missingPathSegments)
        {
            return new PathSelectionResult(
                PathSelectionResultType.MissingFileOrDirectory,
                null,
                foundPathSegments ?? throw new ArgumentNullException(nameof(foundPathSegments)),
                missingPathSegments ?? throw new ArgumentNullException(nameof(missingPathSegments)));
        }

        /// <summary>
        /// Creates a selection for a missing directory.
        /// </summary>
        /// <param name="foundPathSegments">The found path segments.</param>
        /// <param name="missingPathSegments">The missing path elements.</param>
        /// <returns>The created selection result.</returns>
        public static PathSelectionResult CreateMissingDirectory(
            Stack<IUnixDirectoryEntry> foundPathSegments,
            IReadOnlyCollection<string> missingPathSegments)
        {
            return new PathSelectionResult(
                PathSelectionResultType.MissingDirectory,
                null,
                foundPathSegments ?? throw new ArgumentNullException(nameof(foundPathSegments)),
                missingPathSegments ?? throw new ArgumentNullException(nameof(missingPathSegments)));
        }

        /// <summary>
        /// Get the full path as directory entries.
        /// </summary>
        /// <returns>The full path as directory entries.</returns>
        public Stack<IUnixDirectoryEntry> GetPath()
        {
            return new Stack<IUnixDirectoryEntry>(_foundPathSegments.Reverse().SkipWhile(x => x.IsRoot));
        }
    }
}
