using PCL2.Neo.Service.MicrosoftAuth;
using System;

namespace PCL2.Neo.Service.Exceptions
{
    public class DeviceFlowError(
        DeviceFlowState state,
        Exception? exc) : DeviceFlowState
    {
        public DeviceFlowState State { get; init; } = state;
        public Exception? Exc { get; init; } = exc;
    }
}