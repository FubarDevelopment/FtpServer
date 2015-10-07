namespace FubarDev.FtpServer.FileSystem
{
    /// <summary>
    /// The status of a single <see cref="IBackgroundTransfer"/>
    /// </summary>
    public enum BackgroundTransferStatus
    {
        /// <summary>
        /// Added to transfer queue
        /// </summary>
        Enqueued,

        /// <summary>
        /// Transferring the data
        /// </summary>
        Transferring,

        /// <summary>
        /// Transfer finished
        /// </summary>
        Finished,
    }
}
