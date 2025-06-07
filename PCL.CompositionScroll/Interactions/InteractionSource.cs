using Avalonia;
using Avalonia.Input;
using Avalonia.Input.GestureRecognizers;
using System;

namespace CompositionScroll.Interactions
{
    public sealed class InteractionSource : GestureRecognizer
    {
        private static int Id = 1;
        private static int GetId() => Id++;

        private int _interactionId;
        private IPointer _tracking;
        private Visual _rootTarget;
        private Point _pointerPressedPoint;
        private VelocityTracker _velocityTracker;
        private bool _scrolling;

        public InteractionSource(IInputElement target)
        {
            target.PointerWheelChanged += Target_PointerWheelChanged;
        }

        private InteractionTracker Tracker { get; set; }

        public bool CanHorizontallyScroll { get; set; } = false;

        public bool CanVerticallyScroll { get; set; } = true;

        public bool IsScrollInertiaEnabled { get; set; } = true;

        public ScrollFeaturesEnum ScrollFeatures { get; set; } = ScrollFeaturesEnum.None;

        public double ScrollStartDistance { get; set; } = 10;

        private bool CanAny(ScrollFeaturesEnum feature) => (feature & ScrollFeatures) != ScrollFeaturesEnum.None;

        private void Target_PointerWheelChanged(object sender, PointerWheelEventArgs e)
        {
            if (Tracker == null)
                return;

            var delta = e.Delta;

            if (CanAny(ScrollFeaturesEnum.WheelSwapDirections))
            {
                delta = new Vector(delta.Y, delta.X);
            }

            if (e.KeyModifiers == KeyModifiers.Shift)
            {
                delta = new Vector(delta.Y, delta.X);
            }

            delta = FitVector(delta, CanHorizontallyScroll, CanVerticallyScroll);
            delta = delta.Negate();

            delta = delta * 50;

            Tracker.AnimatePositionBy(new Vector3D(delta.X, delta.Y, 0));
            e.Handled = true;
        }

        protected override void PointerPressed(PointerPressedEventArgs e)
        {
            if (e.Pointer.Type == PointerType.Mouse && !CanAny(ScrollFeaturesEnum.MousePressedScroll))
                return;

            EndGesture();
            _tracking = e.Pointer;
            _interactionId = GetId();
            _rootTarget = (Visual)((Target as Visual)?.VisualRoot);
            _pointerPressedPoint = e.GetPosition(_rootTarget);
            _velocityTracker = new VelocityTracker();
            _velocityTracker.AddPosition(TimeSpan.FromMilliseconds(e.Timestamp), default);
            BeginUserInteraction();
        }

        protected override void PointerMoved(PointerEventArgs e)
        {
            if (e.Pointer != _tracking)
                return;

            var rootPoint = e.GetPosition(_rootTarget);
            if (!_scrolling)
            {
                if (CanHorizontallyScroll && Math.Abs(_pointerPressedPoint.X - rootPoint.X) > ScrollStartDistance)
                    _scrolling = true;
                if (CanVerticallyScroll && Math.Abs(_pointerPressedPoint.Y - rootPoint.Y) > ScrollStartDistance)
                    _scrolling = true;

                if (_scrolling)
                {
                    Capture(_tracking);
                }
            }

            if (_scrolling)
            {
                Vector delta = _pointerPressedPoint - rootPoint;
                delta = FitVector(delta, CanHorizontallyScroll, CanVerticallyScroll);

                _velocityTracker?.AddPosition(TimeSpan.FromMilliseconds(e.Timestamp), delta);
                UserInteraction(delta);
                e.Handled = true;
            }
        }

        protected override void PointerReleased(PointerReleasedEventArgs e)
        {
            if (e.Pointer != _tracking)
                return;

            var inertia = _velocityTracker?.GetFlingVelocity().PixelsPerSecond ?? Vector.Zero;
            inertia = FitVector(inertia, CanHorizontallyScroll, CanVerticallyScroll);

            if (_scrolling &&
                IsScrollInertiaEnabled &&
                (e.Pointer.Type != PointerType.Mouse || CanAny(ScrollFeaturesEnum.MousePressedScrollEnertia)))
            {
                EndGesture(inertia);
            }
            else
            {
                EndGesture();
            }
        }

        protected override void PointerCaptureLost(IPointer pointer)
        {
            if (pointer != _tracking)
                return;

            EndGesture();
        }

        private void EndGesture(Vector? inertia = null)
        {
            EndUserInteraction(inertia);
            _tracking = null;
            if (_scrolling)
            {
                _scrolling = false;
                _interactionId = 0;
                _rootTarget = null;
            }
        }

        private static Vector FitVector(Vector vector, bool canHoriz, bool canVer)
        {
            if (!canHoriz)
                vector = new Point(0, vector.Y);
            if (!canVer)
                vector = new Point(vector.X, 0);

            return vector;
        }

        private void BeginUserInteraction()
        {
            Tracker.InteractionStart(_interactionId);
        }

        private void UserInteraction(Vector delta)
        {
            var delta3D = new Vector3D(delta.X, delta.Y, 0);
            Tracker.InteractionMove(_interactionId, delta3D);
        }

        private void EndUserInteraction(Vector? inertia = null)
        {
            if (inertia == null)
                Tracker.InteractionEnd(_interactionId);
            else
            {
                var inertia3D = new Vector3D(inertia.Value.X, inertia.Value.Y, 0);
                Tracker.InteractionEndWithInertia(_interactionId, inertia3D);
            }
        }

        internal void SetInteractionTracker(InteractionTracker tracker)
        {
            Tracker = tracker;
        }
    }
}