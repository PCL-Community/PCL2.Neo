using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Reactive;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using Avalonia.Utilities;
using Avalonia.VisualTree;
using CompositionScroll.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CompositionScroll
{
    [Flags]
    public enum ScrollFeaturesEnum
    {
        None = 0,
        MousePressedScroll = 1,
        MousePressedScrollEnertia = 2,
        WheelSwapDirections = 4,
    }

    /// <summary>
    /// Presents a scrolling view of content inside a <see cref="ScrollViewer"/>.
    /// </summary>
    public sealed class CompositionScrollContentPresenter : ContentPresenter, IScrollable, IScrollAnchorProvider, IInteractionTrackerOwner
    {
        private const double EdgeDetectionTolerance = 0.1;

        public static readonly AttachedProperty<ScrollFeaturesEnum> ScrollFeaturesProperty =
            AvaloniaProperty.RegisterAttached<CompositionScrollContentPresenter, Control, ScrollFeaturesEnum>("ScrollFeatures", defaultValue: ScrollFeaturesEnum.None);

        /// <summary>
        /// Defines the <see cref="CanHorizontallyScroll"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> CanHorizontallyScrollProperty =
            AvaloniaProperty.Register<CompositionScrollContentPresenter, bool>(nameof(CanHorizontallyScroll));

        /// <summary>
        /// Defines the <see cref="CanVerticallyScroll"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> CanVerticallyScrollProperty =
            AvaloniaProperty.Register<CompositionScrollContentPresenter, bool>(nameof(CanVerticallyScroll));

        /// <summary>
        /// Defines the <see cref="Extent"/> property.
        /// </summary>
        public static readonly DirectProperty<CompositionScrollContentPresenter, Size> ExtentProperty =
            ScrollViewer.ExtentProperty.AddOwner<CompositionScrollContentPresenter>(
                o => o.Extent);

        /// <summary>
        /// Defines the <see cref="Offset"/> property.
        /// </summary>
        public static readonly StyledProperty<Vector> OffsetProperty =
            ScrollViewer.OffsetProperty.AddOwner<CompositionScrollContentPresenter>(new(coerce: ScrollViewer.CoerceOffset));

        /// <summary>
        /// Defines the <see cref="Viewport"/> property.
        /// </summary>
        public static readonly DirectProperty<CompositionScrollContentPresenter, Size> ViewportProperty =
            ScrollViewer.ViewportProperty.AddOwner<CompositionScrollContentPresenter>(
                o => o.Viewport);

        /// <summary>
        /// Defines the <see cref="HorizontalSnapPointsType"/> property.
        /// </summary>
        public static readonly StyledProperty<SnapPointsType> HorizontalSnapPointsTypeProperty =
            ScrollViewer.HorizontalSnapPointsTypeProperty.AddOwner<CompositionScrollContentPresenter>();

        /// <summary>
        /// Defines the <see cref="VerticalSnapPointsType"/> property.
        /// </summary>
        public static readonly StyledProperty<SnapPointsType> VerticalSnapPointsTypeProperty =
           ScrollViewer.VerticalSnapPointsTypeProperty.AddOwner<CompositionScrollContentPresenter>();

        /// <summary>
        /// Defines the <see cref="HorizontalSnapPointsAlignment"/> property.
        /// </summary>
        public static readonly StyledProperty<SnapPointsAlignment> HorizontalSnapPointsAlignmentProperty =
            ScrollViewer.HorizontalSnapPointsAlignmentProperty.AddOwner<CompositionScrollContentPresenter>();

        /// <summary>
        /// Defines the <see cref="VerticalSnapPointsAlignment"/> property.
        /// </summary>
        public static readonly StyledProperty<SnapPointsAlignment> VerticalSnapPointsAlignmentProperty =
            ScrollViewer.VerticalSnapPointsAlignmentProperty.AddOwner<CompositionScrollContentPresenter>();

        /// <summary>
        /// Defines the <see cref="IsScrollChainingEnabled"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> IsScrollChainingEnabledProperty =
            ScrollViewer.IsScrollChainingEnabledProperty.AddOwner<CompositionScrollContentPresenter>();

        private ScrollFeaturesEnum _scrollFeatures = ScrollFeaturesEnum.None;
        private InteractionTracker _interactionTracker;
        private ExpressionAnimation _scrollAnimation;
        private bool _compositionUpdate;
        private long? requestId;
        private ScrollPropertiesSource _scrollPropertiesSource;

        private bool _arranging;
        private Size _extent;
        private Size _viewport;
        private HashSet<Control>? _anchorCandidates;
        private Control? _anchorElement;
        private Rect _anchorElementBounds;
        private bool _isAnchorElementDirty;
        private bool _areVerticalSnapPointsRegular;
        private bool _areHorizontalSnapPointsRegular;
        private IReadOnlyList<double>? _horizontalSnapPoints;
        private double _horizontalSnapPoint;
        private IReadOnlyList<double>? _verticalSnapPoints;
        private double _verticalSnapPoint;
        private double _verticalSnapPointOffset;
        private double _horizontalSnapPointOffset;
        private CompositeDisposable? _ownerSubscriptions;
        private ScrollViewer? _owner;
        private IScrollSnapPointsInfo? _scrollSnapPointsInfo;
        private bool _isSnapPointsUpdated;
        private InteractionTrackerInertiaStateEnteredArgs _inertiaArgs;

        private readonly InteractionSource _interactionGesture;

        /// <summary>
        /// Initializes static members of the <see cref="CompositionScrollContentPresenter"/> class.
        /// </summary>
        static CompositionScrollContentPresenter()
        {
            ClipToBoundsProperty.OverrideDefaultValue(typeof(CompositionScrollContentPresenter), true);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositionScrollContentPresenter"/> class.
        /// </summary>
        public CompositionScrollContentPresenter()
        {
            AddHandler(RequestBringIntoViewEvent, BringIntoViewRequested);
            _interactionGesture = new InteractionSource(this);
            GestureRecognizers.Add(_interactionGesture);
        }

        public static ScrollFeaturesEnum GetScrollFeatures(Control element)
        {
            return element.GetValue(ScrollFeaturesProperty);
        }

        public static void SetScrollFeatures(Control element, ScrollFeaturesEnum value)
        {
            element.SetValue(ScrollFeaturesProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the content can be scrolled horizontally.
        /// </summary>
        public bool CanHorizontallyScroll
        {
            get => GetValue(CanHorizontallyScrollProperty);
            set => SetValue(CanHorizontallyScrollProperty, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the content can be scrolled horizontally.
        /// </summary>
        public bool CanVerticallyScroll
        {
            get => GetValue(CanVerticallyScrollProperty);
            set => SetValue(CanVerticallyScrollProperty, value);
        }

        /// <summary>
        /// Gets the extent of the scrollable content.
        /// </summary>
        public Size Extent
        {
            get => _extent;
            private set => SetAndRaise(ExtentProperty, ref _extent, value);
        }

        /// <summary>
        /// Gets or sets the current scroll offset.
        /// </summary>
        public Vector Offset
        {
            get => GetValue(OffsetProperty);
            set => SetValue(OffsetProperty, value);
        }

        /// <summary>
        /// Gets the size of the viewport on the scrollable content.
        /// </summary>
        public Size Viewport
        {
            get => _viewport;
            private set => SetAndRaise(ViewportProperty, ref _viewport, value);
        }

        /// <summary>
        /// Gets or sets how scroll gesture reacts to the snap points along the horizontal axis.
        /// </summary>
        public SnapPointsType HorizontalSnapPointsType
        {
            get => GetValue(HorizontalSnapPointsTypeProperty);
            set => SetValue(HorizontalSnapPointsTypeProperty, value);
        }

        /// <summary>
        /// Gets or sets how scroll gesture reacts to the snap points along the vertical axis.
        /// </summary>
        public SnapPointsType VerticalSnapPointsType
        {
            get => GetValue(VerticalSnapPointsTypeProperty);
            set => SetValue(VerticalSnapPointsTypeProperty, value);
        }

        /// <summary>
        /// Gets or sets how the existing snap points are horizontally aligned versus the initial viewport.
        /// </summary>
        public SnapPointsAlignment HorizontalSnapPointsAlignment
        {
            get => GetValue(HorizontalSnapPointsAlignmentProperty);
            set => SetValue(HorizontalSnapPointsAlignmentProperty, value);
        }

        /// <summary>
        /// Gets or sets how the existing snap points are vertically aligned versus the initial viewport.
        /// </summary>
        public SnapPointsAlignment VerticalSnapPointsAlignment
        {
            get => GetValue(VerticalSnapPointsAlignmentProperty);
            set => SetValue(VerticalSnapPointsAlignmentProperty, value);
        }

        /// <summary>
        ///  Gets or sets if scroll chaining is enabled. The default value is true.
        /// </summary>
        /// <remarks>
        ///  After a user hits a scroll limit on an element that has been nested within another scrollable element,
        /// you can specify whether that parent element should continue the scrolling operation begun in its child element.
        /// This is called scroll chaining.
        /// </remarks>
        public bool IsScrollChainingEnabled
        {
            get => GetValue(IsScrollChainingEnabledProperty);
            set => SetValue(IsScrollChainingEnabledProperty, value);
        }

        /// <inheritdoc/>
        Control? IScrollAnchorProvider.CurrentAnchor
        {
            get
            {
                EnsureAnchorElementSelection();
                return _anchorElement;
            }
        }

        /// <summary>
        /// Attempts to bring a portion of the target visual into view by scrolling the content.
        /// </summary>
        /// <param name="target">The target visual.</param>
        /// <param name="targetRect">The portion of the target visual to bring into view.</param>
        /// <returns>True if the scroll offset was changed; otherwise false.</returns>
        public bool BringDescendantIntoView(Visual target, Rect targetRect)
        {
            if (Child?.IsEffectivelyVisible != true)
            {
                return false;
            }

            var control = target as Control;

            var transform = target.TransformToVisual(Child);

            if (transform == null)
            {
                return false;
            }

            var rectangle = targetRect.TransformToAABB(transform.Value).Deflate(new Thickness(Child.Margin.Left, Child.Margin.Top, 0, 0));
            Rect viewport = new Rect(Offset.X, Offset.Y, Viewport.Width, Viewport.Height);

            double minX = ComputeScrollOffsetWithMinimalScroll(viewport.Left, viewport.Right, rectangle.Left, rectangle.Right);
            double minY = ComputeScrollOffsetWithMinimalScroll(viewport.Top, viewport.Bottom, rectangle.Top, rectangle.Bottom);
            var offset = new Vector(minX, minY);

            if (Offset.NearlyEquals(offset))
            {
                return false;
            }

            var oldOffset = Offset;
            SetCurrentValue(OffsetProperty, offset);

            // It's possible that the Offset coercion has changed the offset back to its previous value,
            // this is common for floating point rounding errors.
            return !Offset.NearlyEquals(oldOffset);
        }

        /// <summary>
        /// Computes the closest offset to ensure most of the child is visible in the viewport along an axis.
        /// </summary>
        /// <param name="viewportStart">The left or top of the viewport</param>
        /// <param name="viewportEnd">The right or bottom of the viewport</param>
        /// <param name="childStart">The left or top of the child</param>
        /// <param name="childEnd">The right or bottom of the child</param>
        /// <returns></returns>
        internal static double ComputeScrollOffsetWithMinimalScroll(
            double viewportStart,
            double viewportEnd,
            double childStart,
            double childEnd)
        {
            // If child is at least partially above viewport, i.e. top of child is above viewport top and bottom of child is above viewport bottom.
            bool isChildAbove = MathUtilities.LessThan(childStart, viewportStart) && MathUtilities.LessThan(childEnd, viewportEnd);

            // If child is at least partially below viewport, i.e. top of child is below viewport top and bottom of child is below viewport bottom.
            bool isChildBelow = MathUtilities.GreaterThan(childEnd, viewportEnd) && MathUtilities.GreaterThan(childStart, viewportStart);
            bool isChildLarger = (childEnd - childStart) > (viewportEnd - viewportStart);

            // Value if no updates is needed. The child is fully visible in the viewport, or the viewport is completely within the child's bounds
            var res = viewportStart;

            // The child is above the viewport and is smaller than the viewport, or if the child's top is below the viewport top
            // and is larger than the viewport, we align the child top to the top of the viewport
            if ((isChildAbove && !isChildLarger)
               || (isChildBelow && isChildLarger))
            {
                res = childStart;
            }
            // The child is above the viewport and is larger than the viewport, or if the child's smaller but is below the viewport,
            // we align the child's bottom to the bottom of the viewport
            else if (isChildAbove || isChildBelow)
            {
                res = (childEnd - (viewportEnd - viewportStart));
            }

            return res;
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            AttachToScrollViewer();

            var compositionVisual = ElementComposition.GetElementVisual(this);
            _interactionTracker = compositionVisual.Compositor.CreateInteractionTracker(this);
            _interactionTracker.InteractionSource = _interactionGesture;
            UpdateScrollAnimation();
            UpdateInteractionOptions();
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            _interactionTracker?.Dispose();
            _interactionTracker = null;
            _scrollAnimation?.Dispose();
            _scrollAnimation = null;
            _scrollPropertiesSource?.Dispose();
            _scrollPropertiesSource = null;
        }

        /// <summary>
        /// Locates the first <see cref="ScrollViewer"/> ancestor and binds to it. Properties which have been set through other means are not bound.
        /// </summary>
        /// <remarks>
        /// This method is automatically called when the control is attached to a visual tree.
        /// </remarks>
        internal void AttachToScrollViewer()
        {
            var owner = this.FindAncestorOfType<ScrollViewer>();

            if (owner == null)
            {
                _owner = null;
                _ownerSubscriptions?.Dispose();
                _ownerSubscriptions = null;
                return;
            }

            if (owner == _owner)
            {
                return;
            }

            _ownerSubscriptions?.Dispose();
            _owner = owner;

            var subscriptionDisposables = new IDisposable?[]
            {
                IfUnset(CanHorizontallyScrollProperty, p => Bind(p, owner.GetObservable(ScrollViewer.HorizontalScrollBarVisibilityProperty, NotDisabled), BindingPriority.Template)),
                IfUnset(CanVerticallyScrollProperty, p => Bind(p, owner.GetObservable(ScrollViewer.VerticalScrollBarVisibilityProperty, NotDisabled), BindingPriority.Template)),
                IfUnset(OffsetProperty, p => Bind(p, owner.GetBindingObservable(ScrollViewer.OffsetProperty), BindingPriority.Template)),
                IfUnset(IsScrollChainingEnabledProperty, p => Bind(p, owner.GetBindingObservable(ScrollViewer.IsScrollChainingEnabledProperty), BindingPriority.Template)),
                IfUnset(ContentProperty, p => Bind(p, owner.GetBindingObservable(ContentProperty), BindingPriority.Template)),
            }.Where(d => d != null).Cast<IDisposable>().ToArray();

            _ownerSubscriptions = new CompositeDisposable(subscriptionDisposables);

            static bool NotDisabled(ScrollBarVisibility v) => v != ScrollBarVisibility.Disabled;

            IDisposable? IfUnset<T>(T property, Func<T, IDisposable> func) where T : AvaloniaProperty => IsSet(property) ? null : func(property);
        }

        /// <inheritdoc/>
        void IScrollAnchorProvider.RegisterAnchorCandidate(Control element)
        {
            if (!this.IsVisualAncestorOf(element))
            {
                throw new InvalidOperationException(
                    "An anchor control must be a visual descendent of the ScrollContentPresenter.");
            }

            _anchorCandidates ??= new();
            _anchorCandidates.Add(element);
            _isAnchorElementDirty = true;
        }

        /// <inheritdoc/>
        void IScrollAnchorProvider.UnregisterAnchorCandidate(Control element)
        {
            _anchorCandidates?.Remove(element);
            _isAnchorElementDirty = true;

            if (_anchorElement == element)
            {
                _anchorElement = null;
            }
        }

        /// <inheritdoc/>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (Child == null)
            {
                return base.MeasureOverride(availableSize);
            }

            var availableWithPadding = availableSize.Deflate(Padding);
            var constraint = new Size(
                CanHorizontallyScroll ? double.PositiveInfinity : availableWithPadding.Width,
                CanVerticallyScroll ? double.PositiveInfinity : availableWithPadding.Height);

            Child.Measure(constraint);

            if (!_isSnapPointsUpdated)
            {
                _isSnapPointsUpdated = true;
                UpdateSnapPoints();
            }

            return Child.DesiredSize.Inflate(Padding).Constrain(availableSize);
        }

        /// <inheritdoc/>
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Child == null)
            {
                return base.ArrangeOverride(finalSize);
            }

            return ArrangeWithAnchoring(finalSize);
        }

        private Size ArrangeWithAnchoring(Size finalSize)
        {
            var size = new Size(
                CanHorizontallyScroll ? Math.Max(Child!.DesiredSize.Inflate(Padding).Width, finalSize.Width) : finalSize.Width,
                CanVerticallyScroll ? Math.Max(Child!.DesiredSize.Inflate(Padding).Height, finalSize.Height) : finalSize.Height);

            var isAnchoring = Offset.X >= EdgeDetectionTolerance || Offset.Y >= EdgeDetectionTolerance;

            if (isAnchoring)
            {
                // Calculate the new anchor element if necessary.
                EnsureAnchorElementSelection();

                // Do the arrange.
                ArrangeOverrideImpl(size, -Offset);

                // If the anchor moved during the arrange, we need to adjust the offset and do another arrange.
                var anchorShift = TrackAnchor();

                if (anchorShift != default)
                {
                    var newOffset = Offset + anchorShift;
                    var newExtent = Extent;
                    var maxOffset = new Vector(Extent.Width - Viewport.Width, Extent.Height - Viewport.Height);

                    if (newOffset.X > maxOffset.X)
                    {
                        newExtent = newExtent.WithWidth(newOffset.X + Viewport.Width);
                    }

                    if (newOffset.Y > maxOffset.Y)
                    {
                        newExtent = newExtent.WithHeight(newOffset.Y + Viewport.Height);
                    }

                    Extent = newExtent;

                    try
                    {
                        _arranging = true;
                        _compositionUpdate = true;
                        _interactionTracker?.ShiftPositionBy(new Vector3D(anchorShift.X, anchorShift.Y, 0));
                        SetCurrentValue(OffsetProperty, newOffset);
                    }
                    finally
                    {
                        _arranging = false;
                        _compositionUpdate = false;
                    }
                }

                ArrangeOverrideImpl(size, -Offset);
            }
            else
            {
                ArrangeOverrideImpl(size, -Offset);
            }

            Viewport = finalSize;
            Extent = ComputeExtent(finalSize);
            _isAnchorElementDirty = true;

            var scrollableHeight = Extent.Height - Viewport.Height;
            var scrollableWidth = Extent.Width - Viewport.Width;
            _interactionTracker?.SetMaxPosition(new Vector3D((float)scrollableWidth, (float)scrollableHeight, 0));

            return finalSize;
        }

        private Size ComputeExtent(Size viewportSize)
        {
            var childMargin = Child!.Margin + Padding;

            if (Child.UseLayoutRounding)
            {
                var scale = LayoutHelper.GetLayoutScale(Child);
                childMargin = LayoutHelper.RoundLayoutThickness(childMargin, scale, scale);
            }

            var extent = Child!.Bounds.Size.Inflate(childMargin);

            if (MathUtilities.AreClose(extent.Width, viewportSize.Width, LayoutHelper.LayoutEpsilon))
                extent = extent.WithWidth(viewportSize.Width);

            if (MathUtilities.AreClose(extent.Height, viewportSize.Height, LayoutHelper.LayoutEpsilon))
                extent = extent.WithHeight(viewportSize.Height);

            return extent;
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            if (change.Property == OffsetProperty)
            {
                if (!_arranging)
                {
                    InvalidateArrange();
                }

                if (!_compositionUpdate)
                {
                    var offset = change.GetNewValue<Vector>();
                    requestId = _interactionTracker?.UpdatePosition(new Vector3D(offset.X, offset.Y, 0));
                }
                else
                {
                    requestId = null;
                }

                _owner?.SetCurrentValue(OffsetProperty, change.GetNewValue<Vector>());
            }
            else if (change.Property == ChildProperty)
            {
                ChildChanged(change);
            }
            else if (change.Property == HorizontalSnapPointsAlignmentProperty ||
                change.Property == VerticalSnapPointsAlignmentProperty)
            {
                UpdateSnapPoints();
            }
            else if (change.Property == ExtentProperty)
            {
                if (_owner != null)
                {
                    _owner.Extent = change.GetNewValue<Size>();
                }
                CoerceValue(OffsetProperty);
            }
            else if (change.Property == ViewportProperty)
            {
                if (_owner != null)
                {
                    _owner.Viewport = change.GetNewValue<Size>();
                }
                CoerceValue(OffsetProperty);
            }
            else if (change.Property == PaddingProperty)
            {
                _scrollAnimation = null;
                UpdateScrollAnimation();
            }
            else
            if (change.Property == ScrollFeaturesProperty ||
                change.Property == CanVerticallyScrollProperty ||
                change.Property == CanHorizontallyScrollProperty)
                UpdateInteractionOptions();

            base.OnPropertyChanged(change);
        }

        private void ScrollSnapPointsInfoSnapPointsChanged(object? sender, RoutedEventArgs e)
        {
            UpdateSnapPoints();
        }

        private void BringIntoViewRequested(object? sender, RequestBringIntoViewEventArgs e)
        {
            if (e.TargetObject is not null)
                e.Handled = BringDescendantIntoView(e.TargetObject, e.TargetRect);
        }

        private void ChildChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                SetCurrentValue(OffsetProperty, default);
                var compositionVisual = ElementComposition.GetElementVisual(e.OldValue as Control);
                if (compositionVisual != null)
                {
                    compositionVisual.ImplicitAnimations = null;
                }
            }

            UpdateScrollAnimation();
        }

        private void EnsureAnchorElementSelection()
        {
            if (!_isAnchorElementDirty || _anchorCandidates is null)
            {
                return;
            }

            _anchorElement = null;
            _anchorElementBounds = default;
            _isAnchorElementDirty = false;

            var bestCandidate = default(Control);
            var bestCandidateDistance = double.MaxValue;

            // Find the anchor candidate that is scrolled closest to the top-left of this
            // ScrollContentPresenter.
            foreach (var element in _anchorCandidates)
            {
                if (element.IsVisible && GetViewportBounds(element, out var bounds))
                {
                    var distance = (Vector)bounds.Position;
                    var candidateDistance = Math.Abs(distance.Length);

                    if (candidateDistance < bestCandidateDistance)
                    {
                        bestCandidate = element;
                        bestCandidateDistance = candidateDistance;
                    }
                }
            }

            if (bestCandidate != null)
            {
                // We have a candidate, calculate its bounds relative to Child. Because these
                // bounds aren't relative to the ScrollContentPresenter itself, if they change
                // then we know it wasn't just due to scrolling.
                var unscrolledBounds = TranslateBounds(bestCandidate, Child!);
                _anchorElement = bestCandidate;
                _anchorElementBounds = unscrolledBounds;
            }
        }

        private Vector TrackAnchor()
        {
            // If we have an anchor and its position relative to Child has changed during the
            // arrange then that change wasn't just due to scrolling (as scrolling doesn't adjust
            // relative positions within Child).
            if (_anchorElement != null &&
                TranslateBounds(_anchorElement, Child!, out var updatedBounds) &&
                updatedBounds.Position != _anchorElementBounds.Position)
            {
                var offset = updatedBounds.Position - _anchorElementBounds.Position;
                return offset;
            }

            return default;
        }

        private bool GetViewportBounds(Control element, out Rect bounds)
        {
            if (TranslateBounds(element, Child!, out var childBounds))
            {
                // We want the bounds relative to the new Offset, regardless of whether the child
                // control has actually been arranged to this offset yet, so translate first to the
                // child control and then apply Offset rather than translating directly to this
                // control.
                var thisBounds = new Rect(Bounds.Size);
                bounds = new Rect(childBounds.Position - Offset, childBounds.Size);
                return bounds.Intersects(thisBounds);
            }

            bounds = default;
            return false;
        }

        private Rect TranslateBounds(Control control, Control to)
        {
            if (TranslateBounds(control, to, out var bounds))
            {
                return bounds;
            }

            throw new InvalidOperationException("The control's bounds could not be translated to the requested control.");
        }

        private bool TranslateBounds(Control control, Control to, out Rect bounds)
        {
            if (!control.IsVisible)
            {
                bounds = default;
                return false;
            }

            var p = control.TranslatePoint(default, to);
            bounds = p.HasValue ? new Rect(p.Value, control.Bounds.Size) : default;
            return p.HasValue;
        }

        private void UpdateSnapPoints()
        {
            var scrollable = GetScrollSnapPointsInfo(Content);

            if (scrollable is IScrollSnapPointsInfo scrollSnapPointsInfo)
            {
                _areVerticalSnapPointsRegular = scrollSnapPointsInfo.AreVerticalSnapPointsRegular;
                _areHorizontalSnapPointsRegular = scrollSnapPointsInfo.AreHorizontalSnapPointsRegular;

                if (!_areVerticalSnapPointsRegular)
                {
                    _verticalSnapPoints = scrollSnapPointsInfo.GetIrregularSnapPoints(Orientation.Vertical, VerticalSnapPointsAlignment);
                }
                else
                {
                    _verticalSnapPoints = new List<double>();
                    _verticalSnapPoint = scrollSnapPointsInfo.GetRegularSnapPoints(Orientation.Vertical, VerticalSnapPointsAlignment, out _verticalSnapPointOffset);

                }

                if (!_areHorizontalSnapPointsRegular)
                {
                    _horizontalSnapPoints = scrollSnapPointsInfo.GetIrregularSnapPoints(Orientation.Horizontal, HorizontalSnapPointsAlignment);
                }
                else
                {
                    _horizontalSnapPoints = new List<double>();
                    _horizontalSnapPoint = scrollSnapPointsInfo.GetRegularSnapPoints(Orientation.Horizontal, HorizontalSnapPointsAlignment, out _horizontalSnapPointOffset);
                }
            }
            else
            {
                _horizontalSnapPoints = new List<double>();
                _verticalSnapPoints = new List<double>();
            }

            UpdateScrollModified();
        }

        private void UpdateScrollModified()
        {
            if (_inertiaArgs == null)
                return;

            var pos = new Vector(_inertiaArgs.NaturalRestingPosition.X, _inertiaArgs.NaturalRestingPosition.Y);

            Vector snapPoint;
            if (_inertiaArgs.IsInertiaFromImpulse)
            {
                var vel = new Vector(-_inertiaArgs.PositionVelocityInPixelsPerSecond.X, -_inertiaArgs.PositionVelocityInPixelsPerSecond.Y);
                snapPoint = SnapOffset(pos, vel, true);
            }
            else
            {
                snapPoint = SnapOffset(pos);
            }

            if (snapPoint == pos)
                return;

            _interactionTracker.AnimatePositionTo(new Vector3D(snapPoint.X, snapPoint.Y, 0));
        }

        private Vector SnapOffset(Vector offset, Vector direction = default, bool snapToNext = false)
        {
            var scrollable = GetScrollSnapPointsInfo(Content);

            if (scrollable is null || (VerticalSnapPointsType == SnapPointsType.None && HorizontalSnapPointsType == SnapPointsType.None))
                return offset;

            var diff = GetAlignmentDiff();

            if (VerticalSnapPointsType != SnapPointsType.None && (_areVerticalSnapPointsRegular || _verticalSnapPoints?.Count > 0) && (!snapToNext || snapToNext && direction.Y != 0))
            {
                var estimatedOffset = new Vector(offset.X, offset.Y + diff.Y);
                double previousSnapPoint = 0, nextSnapPoint = 0, midPoint = 0;

                if (_areVerticalSnapPointsRegular)
                {
                    previousSnapPoint = (int)(estimatedOffset.Y / _verticalSnapPoint) * _verticalSnapPoint + _verticalSnapPointOffset;
                    nextSnapPoint = previousSnapPoint + _verticalSnapPoint;
                    midPoint = (previousSnapPoint + nextSnapPoint) / 2;
                }
                else if (_verticalSnapPoints?.Count > 0)
                {
                    (previousSnapPoint, nextSnapPoint) = FindNearestSnapPoint(_verticalSnapPoints, estimatedOffset.Y);
                    midPoint = (previousSnapPoint + nextSnapPoint) / 2;
                }

                var nearestSnapPoint = snapToNext ? (direction.Y > 0 ? previousSnapPoint : nextSnapPoint) :
                    estimatedOffset.Y < midPoint ? previousSnapPoint : nextSnapPoint;

                offset = new Vector(offset.X, nearestSnapPoint - diff.Y);
            }

            if (HorizontalSnapPointsType != SnapPointsType.None && (_areHorizontalSnapPointsRegular || _horizontalSnapPoints?.Count > 0) && (!snapToNext || snapToNext && direction.X != 0))
            {
                var estimatedOffset = new Vector(offset.X + diff.X, offset.Y);
                double previousSnapPoint = 0, nextSnapPoint = 0, midPoint = 0;

                if (_areHorizontalSnapPointsRegular)
                {
                    previousSnapPoint = (int)(estimatedOffset.X / _horizontalSnapPoint) * _horizontalSnapPoint + _horizontalSnapPointOffset;
                    nextSnapPoint = previousSnapPoint + _horizontalSnapPoint;
                    midPoint = (previousSnapPoint + nextSnapPoint) / 2;
                }
                else if (_horizontalSnapPoints?.Count > 0)
                {
                    (previousSnapPoint, nextSnapPoint) = FindNearestSnapPoint(_horizontalSnapPoints, estimatedOffset.X);
                    midPoint = (previousSnapPoint + nextSnapPoint) / 2;
                }

                var nearestSnapPoint = snapToNext ? (direction.X > 0 ? previousSnapPoint : nextSnapPoint) :
                    estimatedOffset.X < midPoint ? previousSnapPoint : nextSnapPoint;

                offset = new Vector(nearestSnapPoint - diff.X, offset.Y);
            }

            Vector GetAlignmentDiff()
            {
                var vector = default(Vector);

                switch (VerticalSnapPointsAlignment)
                {
                    case SnapPointsAlignment.Center:
                        vector += new Vector(0, Viewport.Height / 2);
                        break;
                    case SnapPointsAlignment.Far:
                        vector += new Vector(0, Viewport.Height);
                        break;
                }

                switch (HorizontalSnapPointsAlignment)
                {
                    case SnapPointsAlignment.Center:
                        vector += new Vector(Viewport.Width / 2, 0);
                        break;
                    case SnapPointsAlignment.Far:
                        vector += new Vector(Viewport.Width, 0);
                        break;
                }

                return vector;
            }

            return offset;
        }

        private static (double previous, double next) FindNearestSnapPoint(IReadOnlyList<double> snapPoints, double value)
        {
            var point = snapPoints.BinarySearch(value, Comparer<double>.Default);

            double previousSnapPoint, nextSnapPoint;

            if (point < 0)
            {
                point = ~point;

                previousSnapPoint = snapPoints[Math.Max(0, point - 1)];
                nextSnapPoint = point >= snapPoints.Count ? snapPoints.Last() : snapPoints[Math.Max(0, point)];
            }
            else
            {
                previousSnapPoint = nextSnapPoint = snapPoints[Math.Max(0, point)];
            }

            return (previousSnapPoint, nextSnapPoint);
        }

        private IScrollSnapPointsInfo? GetScrollSnapPointsInfo(object? content)
        {
            var scrollable = content;

            if (Content is ItemsControl itemsControl)
                scrollable = itemsControl.Presenter?.Panel;

            if (Content is ItemsPresenter itemsPresenter)
                scrollable = itemsPresenter.Panel;

            var snapPointsInfo = scrollable as IScrollSnapPointsInfo;

            if (snapPointsInfo != _scrollSnapPointsInfo)
            {
                if (_scrollSnapPointsInfo != null)
                {
                    _scrollSnapPointsInfo.VerticalSnapPointsChanged -= ScrollSnapPointsInfoSnapPointsChanged;
                    _scrollSnapPointsInfo.HorizontalSnapPointsChanged -= ScrollSnapPointsInfoSnapPointsChanged;
                }

                _scrollSnapPointsInfo = snapPointsInfo;

                if (_scrollSnapPointsInfo != null)
                {
                    _scrollSnapPointsInfo.VerticalSnapPointsChanged += ScrollSnapPointsInfoSnapPointsChanged;
                    _scrollSnapPointsInfo.HorizontalSnapPointsChanged += ScrollSnapPointsInfoSnapPointsChanged;
                }
            }

            return snapPointsInfo;
        }

        public void ValuesChanged(InteractionTracker sender, InteractionTrackerValuesChangedArgs args)
        {
            if (args.RequestId != 0 && requestId.HasValue && args.RequestId <= requestId)
                return;
            try
            {
                _compositionUpdate = true;
                SetCurrentValue(OffsetProperty, new Vector(args.Position.X, args.Position.Y));
            }
            finally
            {
                _compositionUpdate = false;
            }
        }

        public void IdleStateEntered(InteractionTracker sender, InteractionTrackerIdleStateEnteredArgs args)
        {
            _inertiaArgs = null;
        }

        public void InertiaStateEntered(InteractionTracker sender, InteractionTrackerInertiaStateEnteredArgs args)
        {
            _inertiaArgs = args;
            UpdateScrollModified();
        }

        public void InteractingStateEntered(InteractionTracker sender, InteractionTrackerInteractingStateEnteredArgs args)
        {
            _inertiaArgs = null;
        }

        private void EnsureScrollAnimation()
        {
            if (_interactionTracker == null)
                return;

            if (_scrollAnimation == null)
            {
                var compositionVisual = ElementComposition.GetElementVisual(this);

                _scrollAnimation = compositionVisual.Compositor.CreateExpressionAnimation();
                _scrollAnimation.Expression = "Vector3(Margin.X, Margin.Y, 0) - Vector3(Tracker.Position.X, Tracker.Position.Y, Tracker.Position.Z)";
                _scrollAnimation.Target = "Offset";
                _scrollAnimation.SetReferenceParameter("Tracker", _interactionTracker);
                var margin = Child.Margin + Padding;
                _scrollAnimation.SetVector2Parameter("Margin", new System.Numerics.Vector2((float)margin.Left, (float)margin.Top));
            }
        }

        private void UpdateScrollAnimation()
        {
            if (Child == null)
                return;

            var vis = ElementComposition.GetElementVisual(Child);
            if (vis == null)
                return;

            EnsureScrollAnimation();
            if (_scrollAnimation == null)
                return;

            vis.StartAnimation("Offset", _scrollAnimation);
        }

        private void UpdateInteractionOptions()
        {
            if (_interactionTracker == null)
                return;

            var source = _interactionTracker.InteractionSource;
            if (source == null)
                return;

            source.CanVerticallyScroll = CanVerticallyScroll;
            source.CanHorizontallyScroll = CanHorizontallyScroll;
            source.IsScrollInertiaEnabled = ScrollViewer.GetIsScrollInertiaEnabled(this);
            source.ScrollFeatures = GetScrollFeatures(this);
        }

        public ScrollPropertiesSource GetScrollPropertiesSource() => _scrollPropertiesSource ?? CreateScrollPropertiesSource();

        private ScrollPropertiesSource CreateScrollPropertiesSource()
        {
            if (_scrollPropertiesSource == null &&
                CompositionVisual != null &&
                _interactionTracker != null)
            {
                _scrollPropertiesSource = ScrollPropertiesSource.Create(this, _interactionTracker);
            }


            return _scrollPropertiesSource;
        }
    }
}