using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;

namespace CompositionScroll.Interactions
{
    public abstract class InteractionTrackerInertiaModifier : CompositionObject
    {
        internal InteractionTrackerInertiaModifier(Compositor compositor) : base(compositor, null)
        {
        }
    }

    public sealed class InteractionTrackerInertiaRestingValue : InteractionTrackerInertiaModifier
    {
        internal InteractionTrackerInertiaRestingValue(Compositor compositor) : base(compositor)
        {
        }

        public ExpressionAnimation Condition { get; set; }

        public ExpressionAnimation Value { get; set; }

        public static InteractionTrackerInertiaRestingValue Create(Compositor compositor)
            => new(compositor);
    }
}