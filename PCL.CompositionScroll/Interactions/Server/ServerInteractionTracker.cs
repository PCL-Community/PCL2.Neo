using Avalonia;
using Avalonia.Rendering.Composition.Expressions;
using Avalonia.Rendering.Composition.Server;
using Avalonia.Rendering.Composition.Transport;
using System;
using System.IO;

namespace CompositionScroll.Interactions.Server
{
    internal sealed partial class ServerInteractionTracker : ServerObject, IDisposable, IServerClockItem
    {
        internal static CompositionProperty<Vector3D> IdOfPositionProperty =
            CompositionProperty.Register<ServerInteractionTracker, Vector3D>(
                "Position",
                obj => ((ServerInteractionTracker)obj)._position,
                (obj, v) => ((ServerInteractionTracker)obj)._position = v,
                obj => ((ServerInteractionTracker)obj)._position);
        internal static CompositionProperty IdOfMinPositionProperty =
            CompositionProperty.Register<ServerInteractionTracker, Vector3D>(
                "MinPosition",
                obj => ((ServerInteractionTracker)obj)._minPosition,
                (obj, v) => ((ServerInteractionTracker)obj)._minPosition = v,
                obj => ((ServerInteractionTracker)obj)._minPosition);
        internal static CompositionProperty IdOfMaxPositionProperty =
            CompositionProperty.Register<ServerInteractionTracker, Vector3D>(
                "MaxPosition",
                obj => ((ServerInteractionTracker)obj)._maxPosition,
                (obj, v) => ((ServerInteractionTracker)obj)._maxPosition = v,
                obj => ((ServerInteractionTracker)obj)._maxPosition);

        private Vector3D _position;
        private Vector3D _minPosition;
        private Vector3D _maxPosition;

        private enum State
        {
            Idle,
            Inertia,
            Interaction
        }

        private Scroller _scroller;
        private InteractionScroller _interactionScroller;
        private State _state = State.Idle;
        private InteractionTracker _interactionTracker;

        private bool _maxminInvalidated = false;

        public ServerInteractionTracker(ServerCompositor compositor)
            : base(compositor)
        {
            _scroller = new Scroller(this);
            _interactionScroller = new();
        }

        public Vector3D Position
        {
            get => _position;
            private set => SetAnimatedValue(IdOfPositionProperty, out _position, value);
        }

        public Vector3D MinPosition
        {
            get => _minPosition;
            set => SetAnimatedValue(IdOfMinPositionProperty, out _minPosition, value);
        }

        public Vector3D MaxPosition
        {
            get => _maxPosition;
            set => SetAnimatedValue(IdOfMaxPositionProperty, out _maxPosition, value);
        }

        public void Init(InteractionTracker tracker)
        {
            _interactionTracker = tracker;
            Compositor.Animations.AddToClock(this);
        }

        public void Dispose()
        {
            Compositor.Animations.RemoveFromClock(this);
        }

        public void OnTick()
        {
            switch (_state)
            {
                case State.Idle:
                    if (_maxminInvalidated)
                    {
                        SetPosition(_position, 0);
                        _maxminInvalidated = false;
                    }
                    return;

                case State.Inertia:
                    if (!_scroller.Tick())
                    {
                        SetPosition(_scroller.Position, 0);
                        GoToState(State.Idle);
                    }
                    else if (!SetPosition(_scroller.Position, 0))
                    {
                        _scroller.ForceFinished();
                        GoToState(State.Idle);
                    }
                    return;
                case State.Interaction:
                    SetPosition(_interactionScroller.Position, 0);
                    if (_interactionScroller.Finished)
                    {
                        GoToState(State.Idle);
                    }
                    return;
            }
        }

        private void GoToState(State state, bool force = false)
        {
            if (state == _state && !force)
                return;

            var prevState = _state;
            _state = state;
            switch (state)
            {
                case State.Idle:
                    _interactionTracker.IdleStateEntered(0);
                    break;
                case State.Inertia:
                    _interactionTracker.InertiaStateEntered(prevState != State.Interaction, null, _scroller.EndPosition, _scroller.Velocity, 0);
                    break;
                case State.Interaction:
                    _interactionTracker.InteractingStateEntered(0);
                    break;
            }
        }

        private bool SetPosition(Vector3D newPosition, long requestId)
        {
            var newPositionClamp = Vector3D.Clamp(newPosition, MinPosition, MaxPosition);

            if (_position == newPositionClamp)
                return newPositionClamp == newPosition;

            Position = newPositionClamp;
            _interactionTracker.ValuesChanged(_position, requestId);
            return newPositionClamp == newPosition;
        }

        protected override void DeserializeChangesCore(BatchStreamReader reader, TimeSpan committedAt)
        {
            base.DeserializeChangesCore(reader, committedAt);
            var count = reader.Read<int>();
            for (var c = 0; c < count; c++)
                OnMessage(reader.Read<InteractionTrackerRequest>());
        }

        private void OnMessage(InteractionTrackerRequest request)
        {
            switch (request.Type)
            {
                case RequestType.ShiftPositionBy:
                    {
                        var shift = request.VectorValue.Value;
                        switch (_state)
                        {
                            case State.Idle:
                                SetPosition(_position + shift, 0);
                                break;
                            case State.Inertia:
                                _scroller.Shift(shift);
                                break;
                            case State.Interaction:
                                _interactionScroller.Shift(shift);
                                break;
                        }
                    }
                    break;
                case RequestType.AnimatePositionBy:
                    if (_state != State.Interaction)
                    {
                        _scroller.ForcePosition(_position);
                        _scroller.StartInertia(request.VectorValue.Value, request.TimeSpanValue ?? TimeSpan.FromMilliseconds(250));
                        GoToState(State.Inertia, true);
                    }
                    break;
                case RequestType.AnimatePositionTo:
                    if (_state != State.Interaction)
                    {
                        _scroller.ForcePosition(_position);
                        _scroller.StartInertiaTo(request.VectorValue.Value, request.TimeSpanValue ?? TimeSpan.FromMilliseconds(250));
                        GoToState(State.Inertia, true);
                    }
                    break;
                case RequestType.UpdatePositionTo:
                    if (_state != State.Interaction)
                    {
                        _scroller.ForceFinished();
                        _interactionScroller.EndInteraction();
                        SetPosition(request.VectorValue.Value, request.RequestId);
                    }
                    break;

                // Interaction
                case RequestType.InteractionStart:
                    _scroller.ForceFinished();
                    _interactionScroller.StartInteraction(_position);
                    GoToState(State.Interaction);
                    break;
                case RequestType.InteractionMove:
                    if (_state == State.Interaction)
                    {
                        _interactionScroller.Move(request.VectorValue.Value);
                    }
                    break;
                case RequestType.InteractionEnd:
                    if (_state == State.Interaction)
                    {
                        _interactionScroller.EndInteraction();
                        GoToState(State.Idle);
                    }
                    break;
                case RequestType.InteractionEndWithInertia:
                    if (_state == State.Interaction)
                    {
                        _interactionScroller.EndInteraction();
                        _scroller.ForceFinished();
                        _scroller.ForcePosition(_position);
                        _scroller.StartFlingInertia(request.VectorValue.Value);
                        GoToState(State.Inertia);
                    }
                    break;

                case RequestType.SetMaxPosition:
                    MaxPosition = request.VectorValue.Value;
                    _maxminInvalidated = true;
                    break;
                default:
                    break;
            }
        }

        public override CompositionProperty GetCompositionProperty(string fieldName)
        {
            switch (fieldName)
            {
                case nameof(Position):
                    return IdOfPositionProperty;
                case nameof(MinPosition):
                    return IdOfMinPositionProperty;
                case nameof(MaxPosition):
                    return IdOfMaxPositionProperty;
            }

            return base.GetCompositionProperty(fieldName);
        }
    }
}