// <copyright file="MemberValidationResult.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Security.Claims;

using FubarDev.FtpServer.AccountManagement.Compatibility;

namespace FubarDev.FtpServer.AccountManagement
{
    /// <summary>
    /// Result for a member validation.
    /// </summary>
    public class MemberValidationResult
    {
        private readonly MemberValidationStatus _status;

#pragma warning disable 618
        private readonly IFtpUser? _user;
#pragma warning restore 618

        private readonly ClaimsPrincipal? _ftpUser;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberValidationResult"/> class.
        /// </summary>
        /// <param name="status">The error status for the validation.</param>
        public MemberValidationResult(MemberValidationStatus status)
        {
            if (status == MemberValidationStatus.Anonymous || status == MemberValidationStatus.AuthenticatedUser)
            {
                throw new ArgumentOutOfRangeException(nameof(status), "User object must be specified when validation was successful.");
            }

            _status = status;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberValidationResult"/> class.
        /// </summary>
        /// <param name="status">The success status for the validation.</param>
        /// <param name="user">The validated user.</param>
        [Obsolete("Use the overload accepting a ClaimsPrincipal.")]
        public MemberValidationResult(MemberValidationStatus status, IFtpUser user)
        {
            if (status != MemberValidationStatus.Anonymous && status != MemberValidationStatus.AuthenticatedUser)
            {
                throw new ArgumentOutOfRangeException(nameof(status), "User object must only be specified when validation was successful.");
            }

            _status = status;
            _user = user ?? throw new ArgumentNullException(nameof(user));
            _ftpUser = user.CreateClaimsPrincipal();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberValidationResult"/> class.
        /// </summary>
        /// <param name="status">The success status for the validation.</param>
        /// <param name="user">The validated user.</param>
        public MemberValidationResult(MemberValidationStatus status, ClaimsPrincipal user)
        {
            if (status != MemberValidationStatus.Anonymous && status != MemberValidationStatus.AuthenticatedUser)
            {
                throw new ArgumentOutOfRangeException(nameof(status), "User object must only be specified when validation was successful.");
            }

            _status = status;
            _ftpUser = user ?? throw new ArgumentNullException(nameof(user));
#pragma warning disable 618
            _user = user.CreateUser();
#pragma warning restore 618
        }

        /// <summary>
        /// Gets the status of the validation.
        /// </summary>
        [Obsolete("Use the IsAnonymous extension method for ClaimsPrincipal.")]
        public MemberValidationStatus Status => _status;

        /// <summary>
        /// Gets a value indicating whether the user login succeeded.
        /// </summary>
        public bool IsSuccess => _status == MemberValidationStatus.Anonymous || _status == MemberValidationStatus.AuthenticatedUser;

        /// <summary>
        /// Gets the authenticated user.
        /// </summary>
        [Obsolete("Use the FtpUser property.")]
        public IFtpUser User
        {
            get
            {
                if (!IsSuccess)
                {
                    throw new InvalidOperationException("User is only available when the authentication was successful.");
                }

                return _user!;
            }
        }

        /// <summary>
        /// Gets the FTP user.
        /// </summary>
        public ClaimsPrincipal FtpUser
        {
            get
            {
                if (!IsSuccess)
                {
                    throw new InvalidOperationException("User is only available when the authentication was successful.");
                }

                return _ftpUser!;
            }
        }
    }
}
