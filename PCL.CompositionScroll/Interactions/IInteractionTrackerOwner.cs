using Avalonia;

namespace CompositionScroll.Interactions
{
    public interface IInteractionTrackerOwner
    {
        void ValuesChanged(InteractionTracker sender, InteractionTrackerValuesChangedArgs args);

        void IdleStateEntered(InteractionTracker sender, InteractionTrackerIdleStateEnteredArgs args);

        void InertiaStateEntered(InteractionTracker sender, InteractionTrackerInertiaStateEnteredArgs args);

        void InteractingStateEntered(InteractionTracker sender, InteractionTrackerInteractingStateEnteredArgs args);
    }

    public sealed class InteractionTrackerValuesChangedArgs
    {
        internal InteractionTrackerValuesChangedArgs(Vector3D position, long requestId)
        {
            Position = position;
            RequestId = requestId;
        }

        public Vector3D Position { get; }

        public long RequestId { get; }
    }

    public sealed class InteractionTrackerIdleStateEnteredArgs
    {
        internal InteractionTrackerIdleStateEnteredArgs(long requestId)
        {
            RequestId = requestId;
        }

        public long RequestId { get; }
    }

    public sealed class InteractionTrackerInertiaStateEnteredArgs
    {
        internal InteractionTrackerInertiaStateEnteredArgs(bool fromImpulse, Vector3D? modifiedRestingPosition, Vector3D naturalRestingPosition, Vector3D positionVelocity, long requestId)
        {
            IsInertiaFromImpulse = fromImpulse;
            ModifiedRestingPosition = modifiedRestingPosition;
            NaturalRestingPosition = naturalRestingPosition;
            PositionVelocityInPixelsPerSecond = positionVelocity;
            RequestId = requestId;
        }

        public bool IsInertiaFromImpulse { get; }

        public Vector3D? ModifiedRestingPosition { get; }

        public Vector3D NaturalRestingPosition { get; }

        public Vector3D PositionVelocityInPixelsPerSecond { get; }

        public long RequestId { get; }
    }

    public class InteractionTrackerInteractingStateEnteredArgs
    {
        internal InteractionTrackerInteractingStateEnteredArgs(long requestId)
        {
            RequestId = requestId;
        }

        public long RequestId { get; }
    }
}