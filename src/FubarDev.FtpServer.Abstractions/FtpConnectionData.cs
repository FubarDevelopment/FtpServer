//-----------------------------------------------------------------------
// <copyright file="FtpConnectionData.cs" company="Fubar Development Junker">
//     Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>
// <author>Mark Junker</author>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text;

using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.AccountManagement.Compatibility;
using FubarDev.FtpServer.Features;
using FubarDev.FtpServer.Features.Impl;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.Localization;

using Microsoft.AspNetCore.Http.Features;

namespace FubarDev.FtpServer
{
    /// <summary>
    /// Common data for a <see cref="IFtpConnection"/>.
    /// </summary>
    public sealed class FtpConnectionData : ILocalizationFeature, IFileSystemFeature, IAuthorizationInformationFeature,
        ITransferConfigurationFeature, IMlstFactsFeature, IDisposable
    {
        private readonly IFeatureCollection _featureCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpConnectionData"/> class.
        /// </summary>
        /// <param name="defaultEncoding">The default encoding.</param>
        /// <param name="featureCollection">The feature collection where all features get stored.</param>
        /// <param name="catalogLoader">The catalog loader for the FTP server.</param>
        public FtpConnectionData(
            Encoding defaultEncoding,
            IFeatureCollection featureCollection,
            IFtpCatalogLoader catalogLoader)
        {
            _featureCollection = featureCollection;
            featureCollection.Set<ILocalizationFeature>(new LocalizationFeature(catalogLoader));
            featureCollection.Set<IFileSystemFeature>(new FileSystemFeature());
            featureCollection.Set<IAuthorizationInformationFeature>(new AuthorizationInformationFeature());
            featureCollection.Set<IEncodingFeature>(new EncodingFeature(defaultEncoding));
            featureCollection.Set<ITransferConfigurationFeature>(new TransferConfigurationFeature());
        }

        /// <inheritdoc />
        [Obsolete("Use the IAuthorizationInformationFeature services to get the current status.")]
        public IFtpUser? User
        {
            get => _featureCollection.Get<IAuthorizationInformationFeature>().User;
            set
            {
                var feature = _featureCollection.Get<IAuthorizationInformationFeature>();
                feature.User = value;
                feature.FtpUser = value?.CreateClaimsPrincipal();
            }
        }

        /// <inheritdoc />
        [Obsolete("Use the IAuthorizationInformationFeature services to get the current status.")]
        public ClaimsPrincipal? FtpUser
        {
            get => _featureCollection.Get<IAuthorizationInformationFeature>().FtpUser;
            set
            {
                var feature = _featureCollection.Get<IAuthorizationInformationFeature>();
                feature.FtpUser = value;
                feature.User = value?.CreateUser();
            }
        }

        /// <inheritdoc />
        [Obsolete("Use the IAuthorizationInformationFeature services to get the current status.")]
        public IMembershipProvider? MembershipProvider
        {
            get => _featureCollection.Get<IAuthorizationInformationFeature>().MembershipProvider;
            set
            {
                var feature = _featureCollection.Get<IAuthorizationInformationFeature>();
                feature.MembershipProvider = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the user with the <see cref="User"/>.
        /// is logged in.
        /// </summary>
        [Obsolete("Use the IFtpLoginStateMachine services to get the current status.")]
        public bool IsLoggedIn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user is anonymous.
        /// </summary>
        [Obsolete("An anonymous user object now implements IAnonymousFtpUser.")]
        public bool IsAnonymous { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Encoding"/> for the <c>NLST</c> command.
        /// </summary>
        [Obsolete("Query the information using the IEncodingFeature instead.")]
        public Encoding NlstEncoding
        {
            get => _featureCollection.Get<IEncodingFeature>().NlstEncoding;
            set => _featureCollection.Get<IEncodingFeature>().NlstEncoding = value;
        }

        /// <inheritdoc />
        [Obsolete("Query the information using the IFileSystemFeature instead.")]
        public IUnixFileSystem FileSystem
        {
            get => _featureCollection.Get<IFileSystemFeature>().FileSystem;
            set => _featureCollection.Get<IFileSystemFeature>().FileSystem = value;
        }

        /// <inheritdoc />
        [Obsolete("Query the information using the IFileSystemFeature instead.")]
        public Stack<IUnixDirectoryEntry> Path
        {
            get => _featureCollection.Get<IFileSystemFeature>().Path;
            set => _featureCollection.Get<IFileSystemFeature>().Path = value;
        }

        /// <inheritdoc />
        [Obsolete("Query the information using the IFileSystemFeature instead.")]
        public IUnixDirectoryEntry CurrentDirectory => _featureCollection.Get<IFileSystemFeature>().CurrentDirectory;

        /// <inheritdoc />
        [Obsolete("Query the information using the ILocalizationFeature instead.")]
        public CultureInfo Language
        {
            get => _featureCollection.Get<ILocalizationFeature>().Language;
            set => _featureCollection.Get<ILocalizationFeature>().Language = value;
        }

        /// <inheritdoc />
        [Obsolete("Query the information using the ILocalizationFeature instead.")]
        public ILocalizationCatalog Catalog
        {
            get => _featureCollection.Get<ILocalizationFeature>().Catalog;
            set => _featureCollection.Get<ILocalizationFeature>().Catalog = value;
        }

        /// <inheritdoc />
        [Obsolete("Query the information using the ITransferConfigurationFeature instead.")]
        public FtpTransferMode TransferMode
        {
            get => _featureCollection.Get<ITransferConfigurationFeature>().TransferMode;
            set => _featureCollection.Get<ITransferConfigurationFeature>().TransferMode = value;
        }

        /// <summary>
        /// Gets or sets the address to use for an active data connection.
        /// </summary>
        [Obsolete("This property is not used any more. Use IFtpDataConnectionFeature instead.")]
        public Address? PortAddress { get; set; }

        /// <summary>
        /// Gets or sets the data connection for a passive data transfer.
        /// </summary>
        [Obsolete("This property is not used any more. Use IFtpDataConnectionFeature instead.")]
        public TcpClient? PassiveSocketClient { get; set; }

        /// <summary>
        /// Gets the <see cref="IBackgroundCommandHandler"/> that's required for the <c>ABOR</c> command.
        /// </summary>
        [Obsolete("Query IBackgroundTaskLifetimeFeature to get information about an active background task (if non-null).")]
        public IBackgroundCommandHandler? BackgroundCommandHandler => null;

        /// <summary>
        /// Gets or sets the last used transfer type command.
        /// </summary>
        /// <remarks>
        /// It's not allowed to use PASV when PORT was used previously - and vice versa.
        /// </remarks>
        [Obsolete("The restriction was lifted.")]
        public string? TransferTypeCommandUsed { get; set; }

        /// <summary>
        /// Gets or sets the restart position for appending data to a file.
        /// </summary>
        [Obsolete("Query the information using the IRestCommandFeature instead.")]
        public long? RestartPosition
        {
            get => _featureCollection.Get<IRestCommandFeature?>()?.RestartPosition;
            set
            {
                var feature = _featureCollection.Get<IRestCommandFeature?>();
                if (value != null)
                {
                    if (feature == null)
                    {
                        feature = new RestCommandFeature();
                        _featureCollection.Set(feature);
                    }

                    feature.RestartPosition = value.Value;
                }
                else
                {
                    if (feature != null)
                    {
                        _featureCollection.Set<IRestCommandFeature?>(null);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IUnixFileEntry"/> to use for a <c>RNTO</c> operation.
        /// </summary>
        [Obsolete("Query the information using the IRenameCommandFeature instead.")]
        public SearchResult<IUnixFileSystemEntry>? RenameFrom
        {
            get => _featureCollection.Get<IRenameCommandFeature?>()?.RenameFrom;
            set
            {
                var feature = _featureCollection.Get<IRenameCommandFeature?>();
                if (value != null)
                {
                    if (feature == null)
                    {
                        feature = new RenameCommandFeature(value);
                        _featureCollection.Set(feature);
                    }

                    feature.RenameFrom = value;
                }
                else
                {
                    if (feature != null)
                    {
                        _featureCollection.Set<IRenameCommandFeature?>(null);
                    }
                }
            }
        }

        /// <inheritdoc />
        [Obsolete("Query the information using the IMlstFactsFeature instead.")]
        public ISet<string> ActiveMlstFacts => _featureCollection.Get<IMlstFactsFeature>().ActiveMlstFacts;

        /// <summary>
        /// Gets or sets a delegate that allows the creation of an encrypted stream.
        /// </summary>
        [Obsolete("Query the information using the ISecureConnectionFeature instead.")]
        public CreateEncryptedStreamDelegate? CreateEncryptedStream
        {
            get => _featureCollection.Get<ISecureConnectionFeature>().CreateEncryptedStream;
            set => _featureCollection.Get<ISecureConnectionFeature>().CreateEncryptedStream = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets or sets user data as <c>dynamic</c> object.
        /// </summary>
        [Obsolete("Use IFtpConnection.Features to store custom information.")]
        public dynamic UserData { get; set; } = new ExpandoObject();

        /// <inheritdoc/>
        public void Dispose()
        {
        }

        /// <summary>
        /// Container that implements <see cref="IRestCommandFeature"/>.
        /// </summary>
        private class RestCommandFeature : IRestCommandFeature
        {
            /// <inheritdoc />
            public long RestartPosition { get; set; }
        }

        /// <summary>
        /// Container that implements <see cref="IRenameCommandFeature"/>.
        /// </summary>
        private class RenameCommandFeature : IRenameCommandFeature
        {
            public RenameCommandFeature(SearchResult<IUnixFileSystemEntry> renameFrom)
            {
                RenameFrom = renameFrom;
            }

            /// <inheritdoc />
            public SearchResult<IUnixFileSystemEntry> RenameFrom { get; set; }
        }
    }
}
