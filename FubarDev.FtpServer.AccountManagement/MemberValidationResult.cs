// <copyright file="MemberValidationResult.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.FtpServer.AccountManagement
{
    /// <summary>
    /// Result for a member validation
    /// </summary>
    public class MemberValidationResult
    {
        private readonly FtpUser _user;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberValidationResult"/> class.
        /// </summary>
        /// <param name="status">The error status for the validation</param>
        public MemberValidationResult(MemberValidationStatus status)
        {
            if (status == MemberValidationStatus.Anonymous || status == MemberValidationStatus.AuthenticatedUser)
                throw new ArgumentOutOfRangeException(nameof(status), "User object must be specified when validation was successful.");
            Status = status;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberValidationResult"/> class.
        /// </summary>
        /// <param name="status">The success status for the validation</param>
        /// <param name="user">The validated user</param>
        public MemberValidationResult(MemberValidationStatus status, FtpUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (status != MemberValidationStatus.Anonymous && status != MemberValidationStatus.AuthenticatedUser)
                throw new ArgumentOutOfRangeException(nameof(status), "User object must only be specified when validation was successful.");
            Status = status;
            _user = user;
        }

        /// <summary>
        /// Gets the status of the validation
        /// </summary>
        public MemberValidationStatus Status
        { get; }

        /// <summary>
        /// Gets a value indicating whether the user login succeeded
        /// </summary>
        public bool IsSuccess => Status == MemberValidationStatus.Anonymous || Status == MemberValidationStatus.AuthenticatedUser;

        /// <summary>
        /// Gets the authenticated user.
        /// </summary>
        public FtpUser User
        {
            get
            {
                if (!IsSuccess)
                    throw new InvalidOperationException("User is only available when the authentication was successful.");
                return _user;
            }
        }
    }
}
