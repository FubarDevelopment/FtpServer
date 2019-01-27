//-----------------------------------------------------------------------
// <copyright file="FtpTransferMode.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Text;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// FTP transfer mode (RFC 959, 3.4.).
    /// </summary>
    public sealed class FtpTransferMode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpTransferMode"/> class.
        /// </summary>
        /// <param name="fileType">The file data type of this transfer mode.</param>
        public FtpTransferMode(FtpFileType fileType)
        {
            FileType = fileType;
        }

        private FtpTransferMode(FtpFileType fileType, string interpretationMode)
            : this(fileType)
        {
            ParseInterpretationMode(interpretationMode);
        }

        /// <summary>
        /// Gets the file data type.
        /// </summary>
        public FtpFileType FileType { get; }

        /// <summary>
        /// Gets the interpreter mode.
        /// </summary>
        public FtpFileTypeInterpreterMode? InterpreterMode { get; private set; }

        /// <summary>
        /// Gets the bits of a binary transfer mode.
        /// </summary>
        public int? Bits { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the transfer mode is binary.
        /// </summary>
        public bool IsBinary => FileType == FtpFileType.Image || (FileType == FtpFileType.Local && Bits.GetValueOrDefault() == 8);

        /// <summary>
        /// Parses a transfer mode.
        /// </summary>
        /// <param name="type">The transfer mode to parse.</param>
        /// <returns>The new <see cref="FtpTransferMode"/>.</returns>
        public static FtpTransferMode Parse(string type)
        {
            var fileType = ParseFileType(type[0]);
            return new FtpTransferMode(fileType, type.Substring(1));
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            var result = new StringBuilder();
            switch (FileType)
            {
                case FtpFileType.Ascii:
                    result.Append('A');
                    break;
                case FtpFileType.Ebcdic:
                    result.Append('E');
                    break;
                case FtpFileType.Image:
                    result.Append('I');
                    break;
                case FtpFileType.Local:
                    result.Append('L');
                    break;
                default:
                    result.Append('?');
                    break;
            }

            switch (FileType)
            {
                case FtpFileType.Ascii:
                case FtpFileType.Ebcdic:
                    if (InterpreterMode != null && InterpreterMode != FtpFileTypeInterpreterMode.NonPrint)
                    {
                        switch (InterpreterMode.Value)
                        {
                            case FtpFileTypeInterpreterMode.AsaCarriageControl:
                                result.Append('C');
                                break;
                            case FtpFileTypeInterpreterMode.Telnet:
                                result.Append('T');
                                break;
                            default:
                                result.Append('?');
                                break;
                        }
                    }
                    break;
                case FtpFileType.Local:
                    if (Bits != null)
                    {
                        result.Append(Bits);
                    }
                    break;
                default:
                    result.Append('?');
                    break;
            }

            return result.ToString();
        }

        private static FtpFileType ParseFileType(char fileType)
        {
            switch (char.ToUpperInvariant(fileType))
            {
                case 'A':
                    return FtpFileType.Ascii;
                case 'E':
                    return FtpFileType.Ebcdic;
                case 'I':
                    return FtpFileType.Image;
                case 'L':
                    return FtpFileType.Local;
            }

            throw new NotSupportedException($"Unknown file type \"{fileType}\"");
        }

        private void ParseInterpretationMode(string interpretationMode)
        {
            interpretationMode = interpretationMode.Trim();

            if (FileType == FtpFileType.Ascii || FileType == FtpFileType.Ebcdic)
            {
                if (string.IsNullOrEmpty(interpretationMode))
                {
                    InterpreterMode = FtpFileTypeInterpreterMode.NonPrint;
                }
                else
                {
                    var c = char.ToUpperInvariant(interpretationMode[0]);
                    switch (c)
                    {
                        case 'N':
                            InterpreterMode = FtpFileTypeInterpreterMode.NonPrint;
                            break;
                        case 'T':
                            InterpreterMode = FtpFileTypeInterpreterMode.Telnet;
                            break;
                        case 'C':
                            InterpreterMode = FtpFileTypeInterpreterMode.AsaCarriageControl;
                            break;
                        default:
                            throw new NotSupportedException($"Unknown file type interpretation mode \"{interpretationMode}\" for file type {FileType}");
                    }
                }
            }
            else if (FileType == FtpFileType.Local)
            {
                Bits = Convert.ToInt32(interpretationMode, 10);
            }
            else if (!string.IsNullOrEmpty(interpretationMode))
            {
                throw new NotSupportedException($"Unsupported file type interpretation mode \"{interpretationMode}\" for file type {FileType}");
            }
        }
    }
}
