// <copyright file="SecurityActionResult.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Default status codes for the FTP security extensions according to RFC 2228.
    /// </summary>
    public enum SecurityActionResult
    {
        /// <summary>
        /// User logged in, authorized by security data exchange.
        /// </summary>
        UserLoggedIn = 232,

        /// <summary>
        /// Security data exchange complete.
        /// </summary>
        SecurityDataExchangeComplete = 234,

        /// <summary>
        /// <c>[ADAT=base64data]</c>: This reply indicates that the security data exchange completed successfully.
        /// </summary>
        SecurityDataExchangeSuccessful = 235,

        /// <summary>
        /// <c>[ADAT=base64data]</c>: This reply indicates that the requested security mechanism is ok, and includes security data to be used by the client to construct the next command.
        /// </summary>
        RequestedSecurityMechanismOkay = 334,

        /// <summary>
        /// <c>[ADAT=base64data]</c>: This reply indicates that the security data is acceptable, and more is required to complete the security data exchange.
        /// </summary>
        SecurityDataAcceptable = 335,

        /// <summary>
        /// Username okay, need password.  Challenge is "....".
        /// </summary>
        UsernameOkayNeedPassword = 336,

        /// <summary>
        /// Need some unavailable resource to process security.
        /// </summary>
        ResourceUnavailable = 431,

        /// <summary>
        /// Command protection level denied for policy reasons.
        /// </summary>
        CommandProtectionLevelDenied = 533,

        /// <summary>
        /// Request denied for policy reasons.
        /// </summary>
        RequestDenied = 534,

        /// <summary>
        /// Failed security check (hash, sequence, etc).
        /// </summary>
        FailedSecurityCheck = 535,

        /// <summary>
        /// Requested PROT level not supported by mechanism.
        /// </summary>
        RequestedProtLevelNotSupported = 536,

        /// <summary>
        /// Command protection level not supported by security mechanism.
        /// </summary>
        CommandProtectionLevelNotSupported = 537,
    }
}
