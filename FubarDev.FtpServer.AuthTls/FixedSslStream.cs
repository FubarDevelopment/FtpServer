using System.IO;
using System.Net.Security;
using FubarDev.FtpServer.AuthTls.Utilities;

namespace FubarDev.FtpServer.AuthTls
{
    public class FixedSslStream : SslStream
    {
        public FixedSslStream(Stream innerStream)
            : base(innerStream)
        {
        }
        public FixedSslStream(Stream innerStream, bool leaveInnerStreamOpen)
            : base(innerStream, leaveInnerStreamOpen)
        {
        }
        public FixedSslStream(Stream innerStream, bool leaveInnerStreamOpen, RemoteCertificateValidationCallback userCertificateValidationCallback)
            : base(innerStream, leaveInnerStreamOpen, userCertificateValidationCallback)
        {
        }
        public FixedSslStream(Stream innerStream, bool leaveInnerStreamOpen, RemoteCertificateValidationCallback userCertificateValidationCallback, LocalCertificateSelectionCallback userCertificateSelectionCallback)
            : base(innerStream, leaveInnerStreamOpen, userCertificateValidationCallback, userCertificateSelectionCallback)
        {
        }
        public FixedSslStream(Stream innerStream, bool leaveInnerStreamOpen, RemoteCertificateValidationCallback userCertificateValidationCallback, LocalCertificateSelectionCallback userCertificateSelectionCallback, EncryptionPolicy encryptionPolicy)
            : base(innerStream, leaveInnerStreamOpen, userCertificateValidationCallback, userCertificateSelectionCallback, encryptionPolicy)
        {
        }
        public override void Close()
        {
            try
            {
                SslDirectCall.CloseNotify(this);
            }
            finally
            {
                base.Close();
            }
        }
    }
}
