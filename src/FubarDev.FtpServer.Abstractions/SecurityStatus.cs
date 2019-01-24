using System;
using System.Collections.Generic;
using System.Text;

namespace FubarDev.FtpServer
{
    public enum SecurityStatus
    {
        Unauthenticated,
        NeedSecurityData,
        Authenticated,
        NeedPassword,
        NeedAccount,
        Authorized,
    }
}
