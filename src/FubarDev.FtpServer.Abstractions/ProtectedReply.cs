using System;
using System.Collections.Generic;
using System.Text;

namespace FubarDev.FtpServer
{
    public enum ProtectedReply
    {
        Integrity = 631,
        ConfidentialityAndIntegrity = 632,
        Confidentiality = 633,
    }
}
