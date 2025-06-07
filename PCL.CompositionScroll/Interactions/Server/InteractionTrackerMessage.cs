using Avalonia;
using Avalonia.Rendering.Composition.Transport;
using System;
using System.Collections.Generic;

namespace CompositionScroll.Interactions.Server
{
    internal enum RequestType
    {
        AnimatePositionBy,
        AnimatePositionTo,
        ShiftPositionBy,
        UpdatePositionTo,

        InteractionStart,
        InteractionMove,
        InteractionEnd,
        InteractionEndWithInertia,

        SetMinPosition,
        SetMaxPosition,
    }

    internal readonly struct InteractionTrackerRequest
    {
        public InteractionTrackerRequest(RequestType requestType, Vector3D value, long requestId)
        {
            Type = requestType;
            VectorValue = value;
            RequestId = requestId;
        }

        public RequestType Type { get; init; }

        public Vector3D? VectorValue { get; init; }

        public TimeSpan? TimeSpanValue { get; init; }

        public int? IntValue { get; init; }

        public long RequestId { get; init; }
    }

    internal sealed class InteractionTrackerMessage
    {
        public List<InteractionTrackerRequest> Requests { get; } = new();

        public void Serialize(BatchStreamWriter writer)
        {
            writer.Write(Requests.Count);

            foreach (var request in Requests)
                writer.Write(request);
        }

        public void Clear()
        {
            Requests.Clear();
        }
    }
}