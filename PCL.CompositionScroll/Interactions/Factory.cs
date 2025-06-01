using Avalonia.Rendering.Composition;

namespace CompositionScroll.Interactions
{
    public static class Factory
    {
        public static InteractionTracker CreateInteractionTracker(this Compositor compositor)
        {
            return compositor.CreateInteractionTracker(null);
        }

        public static InteractionTracker CreateInteractionTracker(this Compositor compositor, IInteractionTrackerOwner owner)
        {
            var tracker = new InteractionTracker(compositor, owner);
            tracker.Init();
            return tracker;
        }

        public static InteractionTrackerInertiaRestingValue CreateInteractionTrackerInertiaRestingValue(this Compositor compositor)
            => InteractionTrackerInertiaRestingValue.Create(compositor);
    }
}