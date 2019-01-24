using System;
using System.Collections.Generic;
using System.Text;

namespace FubarDev.FtpServer
{
    public enum SecurityActionResult
    {
        UserLoggedIn = 232,
        SecurityDataExchangeComplete = 234,
        SecurityDataExchangeSuccessful = 235,
        RequestedSecurityMechanismOkay = 334,
        SecurityDataAcceptable = 335,
        UsernameOkayNeedPassword = 336,
        RessourceUnavailable = 431,
        CommandProtectionLevelDenied = 533,
        RequestDenied = 534,
        FailedSecurityCheck = 535,
        RequestedProtLevelNotSupported = 536,
        CommandProtectionLevelNotSupported = 537,
    }
}
