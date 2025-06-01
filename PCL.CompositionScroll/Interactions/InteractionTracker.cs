using Avalonia;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Transport;
using Avalonia.Threading;
using CompositionScroll.Interactions.Server;
using System;
using System.Collections.Generic;
using System.Threading;

namespace CompositionScroll.Interactions
{
    public sealed class InteractionTracker : CompositionObject
    {
        private InteractionSource _interactionSource;

        private readonly IInteractionTrackerOwner _owner;
        private InteractionTrackerMessage _message = new();
        private long _requestId = 0;

        internal InteractionTracker(Compositor compositor, IInteractionTrackerOwner owner)
            : base(compositor, new ServerInteractionTracker(compositor.Server))
        {
            _owner = owner;
        }

        internal void Init()
        {
            var server = Server as ServerInteractionTracker;
            server.Init(this);
        }

        public InteractionSource InteractionSource
        {
            get => _interactionSource;
            set
            {
                if (_interactionSource != null)
                    _interactionSource.SetInteractionTracker(null);
                _interactionSource = value;
                if (_interactionSource != null)
                    _interactionSource.SetInteractionTracker(this);
            }
        }

        public void AnimatePositionBy(Vector3D deltaPosition)
        {
            var requestId = Interlocked.Increment(ref _requestId);
            var request = new InteractionTrackerRequest
            {
                Type = RequestType.AnimatePositionBy,
                VectorValue = deltaPosition,
                RequestId = requestId,
            };
            SendHandlerMessage(request);
        }

        public void AnimatePositionTo(Vector3D offsetPosition)
        {
            var requestId = Interlocked.Increment(ref _requestId);
            var request = new InteractionTrackerRequest
            {
                Type = RequestType.AnimatePositionTo,
                VectorValue = offsetPosition,
                RequestId = requestId,
            };
            SendHandlerMessage(request);
        }

        public void AnimatePositionBy(Vector3D deltaPosition, TimeSpan duration)
        {
            var requestId = Interlocked.Increment(ref _requestId);
            var request = new InteractionTrackerRequest
            {
                Type = RequestType.AnimatePositionBy,
                VectorValue = deltaPosition,
                TimeSpanValue = duration,
                RequestId = requestId,
            };
            SendHandlerMessage(request);
        }

        internal void InteractionStart(int interactionId)
        {
            var requestId = Interlocked.Increment(ref _requestId);
            var request = new InteractionTrackerRequest
            {
                Type = RequestType.InteractionStart,
                RequestId = requestId,
                IntValue = interactionId
            };
            SendHandlerMessage(request);
        }

        internal void InteractionMove(int interactionId, Vector3D delta)
        {
            var requestId = Interlocked.Increment(ref _requestId);
            var request = new InteractionTrackerRequest
            {
                Type = RequestType.InteractionMove,
                RequestId = requestId,
                IntValue = interactionId,
                VectorValue = delta,
            };
            SendHandlerMessage(request);
        }

        internal void InteractionEnd(int interactionId)
        {
            var requestId = Interlocked.Increment(ref _requestId);
            var request = new InteractionTrackerRequest
            {
                Type = RequestType.InteractionEnd,
                RequestId = requestId,
                IntValue = interactionId
            };
            SendHandlerMessage(request);
        }

        internal void InteractionEndWithInertia(int interactionId, Vector3D velocity)
        {
            var requestId = Interlocked.Increment(ref _requestId);
            var request = new InteractionTrackerRequest
            {
                Type = RequestType.InteractionEndWithInertia,
                RequestId = requestId,
                VectorValue = velocity,
                IntValue = interactionId
            };
            SendHandlerMessage(request);
        }

        public void ShiftPositionBy(Vector3D shift)
        {
            var requestId = Interlocked.Increment(ref _requestId);
            SendHandlerMessage(new InteractionTrackerRequest(RequestType.ShiftPositionBy, shift, requestId));
        }

        public long UpdatePosition(Vector3D newPosition)
        {
            var requestId = Interlocked.Increment(ref _requestId);
            SendHandlerMessage(new InteractionTrackerRequest(RequestType.UpdatePositionTo, newPosition, requestId));
            return requestId;
        }

        public void SetMaxPosition(Vector3D maxPosition)
        {
            var requestId = Interlocked.Increment(ref _requestId);
            SendHandlerMessage(new InteractionTrackerRequest(RequestType.SetMaxPosition, maxPosition, requestId));
        }

        private void SendHandlerMessage(InteractionTrackerRequest message)
        {
            _message.Requests.Add(message);
            RegisterForSerialization();
        }

        public override void SerializeChangesCore(BatchStreamWriter writer)
        {
            base.SerializeChangesCore(writer);
            _message.Serialize(writer);
            _message.Clear();
        }

        #region Owner

        private readonly object _valuesChangedLocker = new();
        private InteractionTrackerValuesChangedArgs _valuesChangedArg;

        internal void ValuesChanged(Vector3D position, long requestId)
        {
            if (_owner == null)
                return;

            var arg = new InteractionTrackerValuesChangedArgs(position, requestId);
            bool dispatch = false;
            lock (_valuesChangedLocker)
            {
                dispatch = _valuesChangedArg == null;
                _valuesChangedArg = arg;
            }

            if (dispatch)
                Compositor.Dispatcher.InvokeAsync(UIValuesChanged, DispatcherPriority.Input);
        }

        private void UIValuesChanged()
        {
            InteractionTrackerValuesChangedArgs arg;
            lock (_valuesChangedLocker)
            {
                arg = _valuesChangedArg;
                _valuesChangedArg = null;
            }

            _owner.ValuesChanged(this, arg);
        }

        internal void IdleStateEntered(long requestId)
        {
            if (_owner == null)
                return;

            var arg = new InteractionTrackerIdleStateEnteredArgs(requestId);
            Compositor.Dispatcher.InvokeAsync(() => _owner.IdleStateEntered(this, arg), DispatcherPriority.Input);
        }

        internal void InertiaStateEntered(bool fromImpulse, Vector3D? modifiedRestingPosition, Vector3D naturalRestingPosition, Vector3D positionVelocity, long requestId)
        {
            if (_owner == null)
                return;

            var arg = new InteractionTrackerInertiaStateEnteredArgs(fromImpulse, modifiedRestingPosition, naturalRestingPosition, positionVelocity, requestId);
            Compositor.Dispatcher.InvokeAsync(() => _owner.InertiaStateEntered(this, arg), DispatcherPriority.Input);
        }

        internal void InteractingStateEntered(long requestId)
        {
            if (_owner == null)
                return;

            var arg = new InteractionTrackerInteractingStateEnteredArgs(requestId);
            Compositor.Dispatcher.InvokeAsync(() => _owner.InteractingStateEntered(this, arg), DispatcherPriority.Input);
        }

        #endregion
    }
}