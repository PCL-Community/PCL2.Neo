using Avalonia;
using System;

namespace CompositionScroll.Interactions.Server
{
    internal sealed class InteractionScroller
    {
        private bool _finished;

        private Vector3D _position;
        private Vector3D _startPosition;

        public InteractionScroller()
        {
            _finished = true;
        }

        public Vector3D Position => _position;

        public bool Finished => _finished;

        public void StartInteraction(Vector3D start)
        {
            _finished = false;
            _startPosition = start;
            _position = _startPosition;
        }

        public void Move(Vector3D delta)
        {
            _position = _startPosition + delta;
        }

        public void EndInteraction()
        {
            _finished = true;
        }

        public void Shift(Vector3D shift)
        {
            _startPosition += shift;
            _position += shift;
        }
    }

    internal sealed class Scroller
    {
        private static readonly double INFLEXION = 0.35f;
        private static readonly double SCROLL_FRICTION = 0.010f;
        private static readonly double PHYSICAL_COEF = 64818f;
        private static readonly double DECELERATION_RATE = Math.Log(0.78) / Math.Log(0.9);

        private readonly ServerInteractionTracker _interactionTracker;
        private bool _finished;

        private double _durationSec;
        private TimeSpan _startTime;

        private Vector3D _startVelocity;
        private Vector3D _velocity;
        private Vector3D _acceleration;

        private Vector3D _position;
        private Vector3D _startPosition;
        private Vector3D _endPosition;

        public Scroller(ServerInteractionTracker interactionTracker)
        {
            _interactionTracker = interactionTracker;
            _finished = true;
        }

        public Vector3D Position => _position;

        public Vector3D EndPosition => _endPosition;

        public Vector3D Velocity => _velocity;

        public bool Finished => _finished;

        public bool Tick()
        {
            if (_finished)
                return false;

            var now = _interactionTracker.Compositor.ServerNow;

            var deltaT = now - _startTime;
            var deltaTSec = deltaT.TotalSeconds;

            if (deltaTSec >= _durationSec)
            {
                _position = _endPosition;
                _velocity = new();
                _finished = true;
                return false;
            }
            else
            {
                _position = _startPosition + _startVelocity * deltaTSec + _acceleration * (deltaTSec * deltaTSec / 2);
                _velocity = _startPosition + _acceleration * deltaTSec;
                return true;
            }
        }

        public void ForcePosition(Vector3D position)
        {
            _position = position;
        }

        public void ForceFinished()
        {
            _finished = true;
        }

        public void StartInertiaTo(Vector3D target, TimeSpan duration)
        {
            _finished = false;
            _startPosition = _position;
            _endPosition = target;

            _startTime = _interactionTracker.Compositor.ServerNow;
            _durationSec = duration.TotalSeconds;

            _startVelocity = Vector3D.Divide(_endPosition - _startPosition, 0.5d * _durationSec);
            _velocity = _startVelocity;
            _acceleration = Vector3D.Divide(-_velocity, _durationSec);
        }

        public void StartInertia(Vector3D delta, TimeSpan duration)
        {
            if (_finished)
            {
                _finished = false;
                _startPosition = _position;
                _endPosition = _startPosition + delta;
            }
            else
            {
                _startPosition = _position;
                _endPosition += delta;
            }

            _startTime = _interactionTracker.Compositor.ServerNow;
            _durationSec = duration.TotalSeconds;

            _startVelocity = Vector3D.Divide(_endPosition - _startPosition, 0.5d * _durationSec);
            _velocity = _startVelocity;
            _acceleration = Vector3D.Divide(-_velocity, _durationSec);
        }

        public void StartFlingInertia(Vector3D velocity)
        {
            _finished = false;
            _startTime = _interactionTracker.Compositor.ServerNow;
            _startPosition = _position;

            _durationSec = getSplineFlingDuration(velocity);

            _endPosition = _startPosition + velocity * (_durationSec / 2);

            _startVelocity = velocity;
            _velocity = _startVelocity;
            _acceleration = Vector3D.Divide(-_velocity, _durationSec);
        }

        public void Shift(Vector3D shift)
        {
            _startPosition += shift;
            _endPosition += shift;
            _position += shift;
        }

        private static double getSplineDeceleration(double velocity)
            => Math.Log(INFLEXION * Math.Abs(velocity) / (SCROLL_FRICTION * PHYSICAL_COEF));

        private static double getSplineFlingDuration(Vector3D velocity)
        {
            var lenVelocity = velocity.Length;
            var decel = getSplineDeceleration(lenVelocity);
            double decelMinusOne = DECELERATION_RATE - 1.0;
            return Math.Exp(decel / decelMinusOne);
        }
    }
}