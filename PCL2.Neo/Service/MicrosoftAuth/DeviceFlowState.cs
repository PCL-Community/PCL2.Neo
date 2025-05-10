using PCL2.Neo.Models.Account;
using System;

namespace PCL2.Neo.Service.MicrosoftAuth
{
    public class DeviceFlowState;

    public class DeviceFlowAwaitUser(string userCode, string verificationUri) : DeviceFlowState
    {
        public string UserCode { get; } = userCode;
        public string VerificationUri { get; } = verificationUri;
    }

    public class DeviceFlowPolling : DeviceFlowState;

    public class DeviceFlowDeclined : DeviceFlowState;

    public class DeviceFlowExpired : DeviceFlowState;

    public class DeviceFlowBadVerificationCode : DeviceFlowState;

    public class DeviceFlowGetAccountInfo : DeviceFlowState;

    public class DeviceFlowSucceeded(AccountInfo account) : DeviceFlowState

    {
        public AccountInfo Account { get; } = account;
    }

    public class DeviceFlowUnkonw : DeviceFlowState;

    public class DeviceFlowInternetError : DeviceFlowState;

    public class DeviceFlowJsonError : DeviceFlowState;
}